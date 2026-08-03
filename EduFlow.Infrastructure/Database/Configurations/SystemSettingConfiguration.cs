using EduFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduFlow.Infrastructure.Database.Configurations;

internal sealed class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.Property(s => s.Key).HasMaxLength(200).IsRequired();

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Postgres treats NULL as distinct from NULL in a plain unique index, so a composite
        // (TenantId, Key) index alone would not stop duplicate global rows (TenantId IS NULL).
        // Two partial indexes enforce uniqueness within each bucket instead.
        builder.HasIndex(s => new { s.TenantId, s.Key })
            .IsUnique()
            .HasFilter("\"TenantId\" IS NOT NULL");

        builder.HasIndex(s => s.Key)
            .IsUnique()
            .HasFilter("\"TenantId\" IS NULL");
    }
}
