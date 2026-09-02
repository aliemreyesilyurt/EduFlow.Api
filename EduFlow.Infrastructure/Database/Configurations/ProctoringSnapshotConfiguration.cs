using EduFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduFlow.Infrastructure.Database.Configurations;

internal sealed class ProctoringSnapshotConfiguration : IEntityTypeConfiguration<ProctoringSnapshot>
{
    public void Configure(EntityTypeBuilder<ProctoringSnapshot> builder)
    {
        builder.HasOne<ExamAttempt>()
            .WithMany()
            .HasForeignKey(s => s.ExamAttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.ExamAttemptId, s.CapturedOn });
    }
}
