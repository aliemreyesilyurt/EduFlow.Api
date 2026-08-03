using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Enums;

namespace EduFlow.Domain.Entities;

public sealed class Course : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public Guid InstructorId { get; set; }
    public CourseStatus Status { get; set; } = CourseStatus.Draft;
    public DateTime? PublishedOn { get; set; }
}
