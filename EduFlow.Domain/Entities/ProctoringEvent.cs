using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Enums;

namespace EduFlow.Domain.Entities;

public sealed class ProctoringEvent : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid ExamAttemptId { get; set; }
    public ProctoringEventType Type { get; set; }
    public DateTime OccurredOn { get; set; }
    public string? Details { get; set; }
}
