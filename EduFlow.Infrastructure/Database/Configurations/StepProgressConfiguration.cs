using EduFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduFlow.Infrastructure.Database.Configurations;

internal sealed class StepProgressConfiguration : IEntityTypeConfiguration<StepProgress>
{
    public void Configure(EntityTypeBuilder<StepProgress> builder)
    {
        builder.HasOne<Enrollment>()
            .WithMany()
            .HasForeignKey(sp => sp.EnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Step>()
            .WithMany()
            .HasForeignKey(sp => sp.StepId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(sp => new { sp.EnrollmentId, sp.StepId }).IsUnique();
    }
}
