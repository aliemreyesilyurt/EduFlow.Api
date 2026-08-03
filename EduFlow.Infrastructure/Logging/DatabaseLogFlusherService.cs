using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EduFlow.Infrastructure.Logging;

/// <summary>
/// Drains the log channel and batches inserts into <see cref="LoggingDbContext"/> using a short-lived
/// scope per batch (the channel and this service are singletons; the DbContext is scoped).
/// </summary>
public sealed class DatabaseLogFlusherService(
    Channel<AppLog> channel,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    private const int MaxBatchSize = 200;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var batch = new List<AppLog>(MaxBatchSize);

        await foreach (var entry in channel.Reader.ReadAllAsync(stoppingToken))
        {
            batch.Add(entry);

            while (batch.Count < MaxBatchSize && channel.Reader.TryRead(out var more))
            {
                batch.Add(more);
            }

            await FlushAsync(batch, stoppingToken);
            batch.Clear();
        }
    }

    private async Task FlushAsync(List<AppLog> batch, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var loggingDbContext = scope.ServiceProvider.GetRequiredService<LoggingDbContext>();

            loggingDbContext.AppLogs.AddRange(batch);
            await loggingDbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Logging must never take the app down; drop the batch and keep draining.
        }
    }
}
