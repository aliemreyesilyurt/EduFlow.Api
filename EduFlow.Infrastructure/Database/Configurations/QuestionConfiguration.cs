using EduFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduFlow.Infrastructure.Database.Configurations;

internal sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.HasOne<Exam>()
            .WithMany()
            .HasForeignKey(q => q.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(q => new { q.ExamId, q.Order }).IsUnique();
    }
}
