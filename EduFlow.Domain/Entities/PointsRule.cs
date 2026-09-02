using EduFlow.Domain.Abstractions;

namespace EduFlow.Domain.Entities;

public sealed class PointsRule : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public int PointsCost { get; set; }
    public bool IsActive { get; set; } = true;
}
