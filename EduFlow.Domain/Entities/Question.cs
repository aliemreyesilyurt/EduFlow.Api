using EduFlow.Domain.Abstractions;

namespace EduFlow.Domain.Entities;

public sealed class Question : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid ExamId { get; set; }
    public required string Text { get; set; }
    public int Order { get; set; }
    public int Points { get; set; } = 1;
}
