using EduFlow.Domain.Abstractions;

namespace EduFlow.Domain.Entities;

public sealed class Exam : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid CourseId { get; set; }
    public required string Title { get; set; }
    public int PassScorePercentage { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public int? MaxAttempts { get; set; }
    public bool IsPublished { get; set; }
    public bool ProctoringEnabled { get; set; }
    public bool RequireCamera { get; set; }
    public int? SnapshotIntervalSeconds { get; set; }
    public int? ViolationWarningThreshold { get; set; }
}
