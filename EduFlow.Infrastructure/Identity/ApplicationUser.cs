using Microsoft.AspNetCore.Identity;

namespace EduFlow.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public Guid? TenantId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }

    /// <summary>Encrypted at rest via <see cref="Security.EncryptedStringConverter"/> (KVKK).</summary>
    public string? NationalId { get; set; }

    public DateTime CreatedOn { get; set; }
}
