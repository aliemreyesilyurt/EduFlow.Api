using EduFlow.Application.Abstractions;
using EduFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EduFlow.Infrastructure.Interceptors;

/// <summary>
/// Turns every delete of a <see cref="BaseEntity"/> into an update instead: the row stays in the
/// table with <c>IsDeleted = true</c> and is hidden afterwards by the query filter set up in
/// <see cref="Database.ApplicationDbContext"/>. Runs before <see cref="AuditInterceptor"/> so the
/// converted-to-Modified entry still gets its UpdatedOn/UpdatedBy stamped.
/// </summary>
public sealed class SoftDeleteInterceptor(ITenantContext tenantContext) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplySoftDelete(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplySoftDelete(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplySoftDelete(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State != EntityState.Deleted)
            {
                continue;
            }

            entry.State = EntityState.Modified;
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedOn = DateTime.UtcNow;
            entry.Entity.DeletedBy = tenantContext.UserId;
        }
    }
}
