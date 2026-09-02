using EduFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduFlow.Infrastructure.Database.Configurations;

internal sealed class ProctoringEventConfiguration : IEntityTypeConfiguration<ProctoringEvent>
{
    public void Configure(EntityTypeBuilder<ProctoringEvent> builder)
    {
        builder.HasOne<ExamAttempt>()
            .WithMany()
            .HasForeignKey(e => e.ExamAttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.ExamAttemptId, e.OccurredOn });
    }
}
