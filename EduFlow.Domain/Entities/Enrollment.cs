using EduFlow.Domain.Abstractions;

namespace EduFlow.Domain.Entities;

public sealed class Enrollment : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid CourseId { get; set; }
    public Guid StudentId { get; set; }
    public DateTime EnrolledOn { get; set; }
    public DateTime? CompletedOn { get; set; }
}
