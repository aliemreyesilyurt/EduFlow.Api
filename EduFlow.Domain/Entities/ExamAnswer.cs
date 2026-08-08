using EduFlow.Domain.Abstractions;

namespace EduFlow.Domain.Entities;

public sealed class ExamAnswer : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid ExamAttemptId { get; set; }
    public Guid QuestionId { get; set; }
    public Guid? SelectedOptionId { get; set; }
}
