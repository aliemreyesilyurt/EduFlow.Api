using System.Reflection;
using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Security;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;
using EduFlow.Infrastructure.Database.Configurations;
using EduFlow.Infrastructure.Identity;
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

    private static readonly MethodInfo SetSoftDeleteQueryFilterMethod = typeof(ApplicationDbContext)
        .GetMethod(nameof(SetSoftDeleteQueryFilter), BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static readonly MethodInfo SetTenantAndSoftDeleteQueryFilterMethod = typeof(ApplicationDbContext)
        .GetMethod(nameof(SetTenantAndSoftDeleteQueryFilter), BindingFlags.NonPublic | BindingFlags.Instance)!;

    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<Course> Courses { get; set; } = null!;
    public DbSet<Step> Steps { get; set; } = null!;
    public DbSet<Enrollment> Enrollments { get; set; } = null!;
    public DbSet<StepProgress> StepProgresses { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    public DbSet<Rating> Ratings { get; set; } = null!;
    public DbSet<SystemSetting> SystemSettings { get; set; } = null!;
    public DbSet<Exam> Exams { get; set; } = null!;
    public DbSet<Question> Questions { get; set; } = null!;
    public DbSet<QuestionOption> QuestionOptions { get; set; } = null!;
    public DbSet<ExamAttempt> ExamAttempts { get; set; } = null!;
    public DbSet<ExamAnswer> ExamAnswers { get; set; } = null!;
    public DbSet<ProctoringEvent> ProctoringEvents { get; set; } = null!;
    public DbSet<ProctoringSnapshot> ProctoringSnapshots { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ApplicationUserConfiguration needs the request-scoped ICryptographyService, which a
        // parameterless configuration type can't receive, so it's excluded here and applied manually.
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly,
            type => type != typeof(ApplicationUserConfiguration));

        modelBuilder.ApplyConfiguration(new ApplicationUserConfiguration(cryptographyService));

        // Tenant-scoped business entities (ITenantEntity) get an automatic row-level filter, and
        // every BaseEntity (ISoftDelete) gets a filter hiding soft-deleted rows. ApplicationUser is
        // intentionally excluded from tenant scoping: Identity's UserManager needs to look users up
        // by email during login, before any tenant is known, so it is scoped explicitly per use case
        // instead of through a blanket global filter.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            var isTenantScoped = typeof(ITenantEntity).IsAssignableFrom(clrType);
            var isSoftDeletable = typeof(ISoftDelete).IsAssignableFrom(clrType);

            if (isTenantScoped && isSoftDeletable)
            {
                SetTenantAndSoftDeleteQueryFilterMethod.MakeGenericMethod(clrType).Invoke(this, [modelBuilder]);
            }
            else if (isTenantScoped)
            {
                SetTenantQueryFilterMethod.MakeGenericMethod(clrType).Invoke(this, [modelBuilder]);
            }
            else if (isSoftDeletable)
            {
                SetSoftDeleteQueryFilterMethod.MakeGenericMethod(clrType).Invoke(this, [modelBuilder]);
            }
        }
    }

    private void SetTenantQueryFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantEntity
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(e => tenantContext.IsSysAdmin || e.TenantId == tenantContext.TenantId);
    }

    private void SetSoftDeleteQueryFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ISoftDelete
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(e => !e.IsDeleted);
    }

    private void SetTenantAndSoftDeleteQueryFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantEntity, ISoftDelete
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(e => !e.IsDeleted && (tenantContext.IsSysAdmin || e.TenantId == tenantContext.TenantId));
    }
}
