using EduFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduFlow.Infrastructure.Database.Configurations;

internal sealed class ExamAnswerConfiguration : IEntityTypeConfiguration<ExamAnswer>
{
    public void Configure(EntityTypeBuilder<ExamAnswer> builder)
    {
        builder.HasOne<ExamAttempt>()
            .WithMany()
            .HasForeignKey(a => a.ExamAttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Question>()
            .WithMany()
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.ExamAttemptId, a.QuestionId }).IsUnique();
    }
}
