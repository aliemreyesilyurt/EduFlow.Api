using EduFlow.Domain.Abstractions;

namespace EduFlow.Domain.Entities;

public sealed class ExamAttempt : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid ExamId { get; set; }
    public Guid StudentId { get; set; }
    public int AttemptNumber { get; set; }
    public DateTime StartedOn { get; set; }
    public DateTime? SubmittedOn { get; set; }
    public double? ScorePercentage { get; set; }
    public bool? Passed { get; set; }
}
