using System.Text.Json;
using EduFlow.Application.Abstractions;
using EduFlow.Domain.Entities;
using EduFlow.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EduFlow.Infrastructure.Interceptors;

/// <summary>
/// Records a Created/Updated/Deleted trail for every <see cref="BaseEntity"/> change into the
/// separate <see cref="LoggingDbContext"/>. Registered last so it observes entities after the other
/// interceptors (soft-delete conversion, CreatedBy/UpdatedBy stamping) have already run.
///
/// The audit rows are written in SavedChanges(Async) — after the main SaveChanges has actually
/// committed — using a snapshot captured during SavingChanges (while OriginalValues are still
/// available). This means the audit write is a secondary effect, not atomic with the business
/// write: if the process dies between the two, the business change is durable but its audit row
/// may be lost. That trade-off is what "separate context" buys you (no coupling to the business
/// schema/migrations) and is acceptable for an audit trail rather than a ledger of record.
/// </summary>
public sealed class AuditLogInterceptor(
    LoggingDbContext loggingDbContext,
    ITenantContext tenantContext) : SaveChangesInterceptor
{
    private static readonly HashSet<string> IgnoredProperties =
    [
        nameof(BaseEntity.CreatedOn), nameof(BaseEntity.CreatedBy),
        nameof(BaseEntity.UpdatedOn), nameof(BaseEntity.UpdatedBy),
        nameof(BaseEntity.DeletedOn), nameof(BaseEntity.DeletedBy),
        nameof(BaseEntity.IsDeleted)
    ];

    private List<AuditLog> _pendingAuditLogs = [];

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        _pendingAuditLogs = BuildAuditLogs(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        _pendingAuditLogs = BuildAuditLogs(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        PersistPendingAuditLogs();

        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await PersistPendingAuditLogsAsync(cancellationToken);

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private List<AuditLog> BuildAuditLogs(DbContext? context)
    {
        if (context is null)
        {
            return [];
        }

        var logs = new List<AuditLog>();

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            var action = ResolveAction(entry);

            if (action is null)
            {
                continue;
            }

            var changes = BuildChanges(entry, action.Value);

            if (changes is null)
            {
                continue;
            }

            logs.Add(new AuditLog
            {
                Id = Guid.CreateVersion7(),
                TenantId = tenantContext.TenantId,
                UserId = tenantContext.UserId,
                EntityName = entry.Entity.GetType().Name,
                EntityId = entry.Entity.Id.ToString(),
                Action = action.Value,
                Changes = changes,
                Timestamp = DateTime.UtcNow
            });
        }

        return logs;
    }

    private static AuditAction? ResolveAction(EntityEntry<BaseEntity> entry) => entry.State switch
    {
        EntityState.Added => AuditAction.Created,
        EntityState.Deleted => AuditAction.Deleted,
        EntityState.Modified when IsBeingSoftDeleted(entry) => AuditAction.Deleted,
        EntityState.Modified => AuditAction.Updated,
        _ => null
    };

    private static bool IsBeingSoftDeleted(EntityEntry<BaseEntity> entry) =>
        entry.Entity.IsDeleted && entry.Property(nameof(BaseEntity.IsDeleted)).IsModified;

    private static string? BuildChanges(EntityEntry<BaseEntity> entry, AuditAction action)
    {
        var relevantProperties = entry.Properties.Where(p => !IgnoredProperties.Contains(p.Metadata.Name));

        switch (action)
        {
            case AuditAction.Created:
                return JsonSerializer.Serialize(
                    relevantProperties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue));

            case AuditAction.Deleted:
                return JsonSerializer.Serialize(
                    relevantProperties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue));

            default:
                var changed = relevantProperties
                    .Where(p => p.IsModified && !Equals(p.OriginalValue, p.CurrentValue))
                    .ToDictionary(p => p.Metadata.Name, p => new { Old = p.OriginalValue, New = p.CurrentValue });

                return changed.Count == 0 ? null : JsonSerializer.Serialize(changed);
        }
    }

    private void PersistPendingAuditLogs()
    {
        if (_pendingAuditLogs.Count == 0)
        {
            return;
        }

        loggingDbContext.AuditLogs.AddRange(_pendingAuditLogs);
        loggingDbContext.SaveChanges();
        _pendingAuditLogs = [];
    }

    private async Task PersistPendingAuditLogsAsync(CancellationToken cancellationToken)
    {
        if (_pendingAuditLogs.Count == 0)
        {
            return;
        }

        loggingDbContext.AuditLogs.AddRange(_pendingAuditLogs);
        await loggingDbContext.SaveChangesAsync(cancellationToken);
        _pendingAuditLogs = [];
    }
}
