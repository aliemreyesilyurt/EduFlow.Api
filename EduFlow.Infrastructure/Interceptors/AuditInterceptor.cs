using EduFlow.Application.Abstractions;
using EduFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EduFlow.Infrastructure.Interceptors;

public sealed class AuditInterceptor(ITenantContext tenantContext) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyAuditInfo(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAuditInfo(DbContext? context)
    {
        if (context == null)
            return;

        foreach (var entry in context.ChangeTracker
                                     .Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedOn = DateTime.UtcNow;
                entry.Entity.CreatedBy = tenantContext.UserId;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedOn = DateTime.UtcNow;
                entry.Entity.UpdatedBy = tenantContext.UserId;
            }
        }
    }
}
