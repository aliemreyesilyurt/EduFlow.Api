using EduFlow.Domain.Abstractions;

namespace EduFlow.Domain.Entities;

public sealed class ProctoringSnapshot : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid ExamAttemptId { get; set; }
    public DateTime CapturedOn { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public long SizeBytes { get; set; }
}
