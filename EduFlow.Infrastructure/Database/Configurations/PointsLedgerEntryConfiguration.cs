using EduFlow.Domain.Entities;
using EduFlow.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduFlow.Infrastructure.Database.Configurations;

internal sealed class PointsLedgerEntryConfiguration : IEntityTypeConfiguration<PointsLedgerEntry>
{
    public void Configure(EntityTypeBuilder<PointsLedgerEntry> builder)
    {
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ExamAttempt>()
            .WithMany()
            .HasForeignKey(e => e.ExamAttemptId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => new { e.StudentId, e.CreatedOn });
    }
}
