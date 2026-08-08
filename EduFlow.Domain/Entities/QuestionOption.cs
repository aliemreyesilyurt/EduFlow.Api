using EduFlow.Domain.Abstractions;

namespace EduFlow.Domain.Entities;

public sealed class QuestionOption : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid QuestionId { get; set; }
    public required string Text { get; set; }
    public bool IsCorrect { get; set; }
    public int Order { get; set; }
}
