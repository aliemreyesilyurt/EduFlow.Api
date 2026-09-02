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
    public DateTime? ProctoringConsentOn { get; set; }
    public int ViolationCount { get; set; }
    public bool RequiresReview { get; set; }
    public bool? ReviewApproved { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedOn { get; set; }
    public string? ReviewNote { get; set; }
    public bool PointsAwarded { get; set; }
}
