namespace EduFlow.Infrastructure.Identity;

public sealed class RefreshToken
{
    public Guid Id { get; set; }
    public required Guid UserId { get; set; }

    /// <summary>SHA-256 hash of the token; the raw token is never persisted.</summary>
    public required string TokenHash { get; set; }

    public DateTime CreatedOn { get; set; }
    public DateTime ExpiresOn { get; set; }
    public DateTime? RevokedOn { get; set; }

    public bool IsActive => RevokedOn is null && ExpiresOn > DateTime.UtcNow;
}
