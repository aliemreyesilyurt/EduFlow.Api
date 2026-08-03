using EduFlow.Application.Abstractions.Security;
using EduFlow.Infrastructure.Identity;
using EduFlow.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduFlow.Infrastructure.Database.Configurations;

/// <summary>
/// Not picked up by <see cref="ModelBuilder.ApplyConfigurationsFromAssembly"/> (excluded via predicate)
/// because it needs the request-scoped <see cref="ICryptographyService"/>, which a parameterless
/// configuration type can't receive. Applied manually in <see cref="ApplicationDbContext.OnModelCreating"/>.
/// </summary>
internal sealed class ApplicationUserConfiguration(ICryptographyService cryptographyService)
    : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.NationalId)
            .HasConversion(new EncryptedStringConverter(cryptographyService));
    }
}
