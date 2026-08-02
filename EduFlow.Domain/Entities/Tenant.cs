namespace EduFlow.Domain.Entities;

public sealed class Tenant : AuditableEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public bool IsActive { get; set; } = true;
}
