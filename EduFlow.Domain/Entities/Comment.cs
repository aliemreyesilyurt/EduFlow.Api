using EduFlow.Domain.Abstractions;

namespace EduFlow.Domain.Entities;

public sealed class Comment : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid CourseId { get; set; }
    public Guid? StepId { get; set; }
    public Guid AuthorId { get; set; }
    public required string Content { get; set; }
    public bool IsHidden { get; set; }
}
