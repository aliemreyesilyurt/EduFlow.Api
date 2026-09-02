using EduFlow.Application.Abstractions.Storage;
using EduFlow.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EduFlow.Infrastructure.Proctoring;

/// <summary>
/// Sweeps proctoring snapshots past their owning tenant's <c>ProctoringRetentionDays</c> once a
/// day, deleting both the DB row and the stored file. Runs with a scoped DbContext queried via
/// <c>IgnoreQueryFilters()</c> since there is no HttpContext/tenant in a background scope, so the
/// automatic tenant query filter would otherwise hide every row.
/// </summary>
public sealed class ProctoringRetentionService(
    IServiceScopeFactory scopeFactory,
    ILogger<ProctoringRetentionService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromDays(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        do
        {
            await RunOnceAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var fileStorage = scope.ServiceProvider.GetRequiredService<IFileStorage>();

            var now = DateTime.UtcNow;

            var expiredSnapshots = await dbContext.ProctoringSnapshots
                .IgnoreQueryFilters()
                .Join(
                    dbContext.Tenants.IgnoreQueryFilters(),
                    snapshot => snapshot.TenantId,
                    tenant => tenant.Id,
                    (snapshot, tenant) => new { snapshot, tenant.ProctoringRetentionDays })
                .Where(x => x.snapshot.CapturedOn < now.AddDays(-x.ProctoringRetentionDays))
                .Select(x => x.snapshot)
                .ToListAsync(cancellationToken);

            foreach (var snapshot in expiredSnapshots)
            {
                await fileStorage.DeleteFileAsync(
                    $"proctoring/{snapshot.ExamAttemptId}", snapshot.FileName, cancellationToken);
            }

            if (expiredSnapshots.Count > 0)
            {
                dbContext.ProctoringSnapshots.RemoveRange(expiredSnapshots);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Proctoring snapshot retention sweep failed");
        }
    }
}
