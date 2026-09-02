using EduFlow.Domain.Entities;
using EduFlow.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduFlow.Infrastructure.Database.Configurations;

internal sealed class PointsWalletConfiguration : IEntityTypeConfiguration<PointsWallet>
{
    public void Configure(EntityTypeBuilder<PointsWallet> builder)
    {
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(w => w.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(w => w.StudentId).IsUnique();
    }
}
