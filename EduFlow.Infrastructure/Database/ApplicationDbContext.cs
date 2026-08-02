using System.Reflection;
using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Security;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;
using EduFlow.Infrastructure.Identity;
using EduFlow.Infrastructure.Security;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EduFlow.Infrastructure.Database;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ITenantContext tenantContext,
    ICryptographyService cryptographyService)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    private static readonly MethodInfo SetTenantQueryFilterMethod = typeof(ApplicationDbContext)
        .GetMethod(nameof(SetTenantQueryFilter), BindingFlags.NonPublic | BindingFlags.Instance)!;

    public DbSet<Book> Books { get; set; } = null!;
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var encryptedStringConverter = new EncryptedStringConverter(cryptographyService);

        modelBuilder.Entity<ApplicationUser>()
            .Property(u => u.NationalId)
            .HasConversion(encryptedStringConverter);

        modelBuilder.Entity<Tenant>()
            .HasIndex(t => t.Slug)
            .IsUnique();

        // Tenant-scoped business entities (ITenantEntity) get an automatic row-level filter.
        // ApplicationUser is intentionally excluded: Identity's UserManager needs to look users up
        // by email during login, before any tenant is known, so it is scoped explicitly per use case
        // instead of through a blanket global filter.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                SetTenantQueryFilterMethod.MakeGenericMethod(entityType.ClrType).Invoke(this, [modelBuilder]);
            }
        }
    }

    private void SetTenantQueryFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantEntity
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(e => tenantContext.IsSysAdmin || e.TenantId == tenantContext.TenantId);
    }
}
