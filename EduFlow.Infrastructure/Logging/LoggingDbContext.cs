using Microsoft.EntityFrameworkCore;

namespace EduFlow.Infrastructure.Logging;

/// <summary>
/// Deliberately separate from <see cref="Database.ApplicationDbContext"/>: audit/log tables are
/// append-only, high-volume, and have nothing to do with the business schema's migrations or
/// query filters, so they get their own context (and their own migrations history table) even
/// though they currently share the same physical database.
/// </summary>
public sealed class LoggingDbContext(DbContextOptions<LoggingDbContext> options) : DbContext(options)
{
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AppLog> AppLogs => Set<AppLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AuditLog>(builder =>
        {
            builder.HasKey(a => a.Id);
            builder.Property(a => a.EntityName).HasMaxLength(200);
            builder.Property(a => a.EntityId).HasMaxLength(200);
            builder.HasIndex(a => new { a.EntityName, a.EntityId });
            builder.HasIndex(a => a.Timestamp);
        });

        modelBuilder.Entity<AppLog>(builder =>
        {
            builder.HasKey(l => l.Id);
            builder.Property(l => l.Level).HasMaxLength(20);
            builder.Property(l => l.Category).HasMaxLength(200);
            builder.HasIndex(l => l.Timestamp);
        });
    }
}
