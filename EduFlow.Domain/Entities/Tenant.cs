namespace EduFlow.Domain.Entities;

public sealed class Tenant : BaseEntity
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public bool IsActive { get; set; } = true;
    public bool AllowSelfRegistration { get; set; }
}
