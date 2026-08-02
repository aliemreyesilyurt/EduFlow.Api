using EduFlow.Application.Abstractions;
using EduFlow.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EduFlow.Infrastructure.Interceptors;

/// <summary>
/// Auto-stamps TenantId on inserted <see cref="ITenantEntity"/> rows from the current request's
/// tenant context. Scoped (not singleton) because it depends on the per-request ITenantContext.
/// </summary>
public sealed class TenantSaveChangesInterceptor(ITenantContext tenantContext) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyTenantId(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyTenantId(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyTenantId(DbContext? context)
    {
        if (context is null || tenantContext.TenantId is not { } tenantId)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries<ITenantEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.TenantId == Guid.Empty)
            {
                entry.Entity.TenantId = tenantId;
            }
        }
    }
}
