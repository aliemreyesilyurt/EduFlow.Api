using EduFlow.Domain.Abstractions;

namespace EduFlow.Domain.Entities;

public sealed class PointsWallet : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid StudentId { get; set; }
    public int Balance { get; set; }
}
