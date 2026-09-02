using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Enums;

namespace EduFlow.Domain.Entities;

public sealed class PointsLedgerEntry : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid StudentId { get; set; }
    public int Amount { get; set; }
    public int BalanceAfter { get; set; }
    public PointsReason Reason { get; set; }
    public string? Description { get; set; }
    public Guid? ExamAttemptId { get; set; }
}
