using EduFlow.Domain.Abstractions;

namespace EduFlow.Domain.Entities;

public sealed class Enrollment : AuditableEntity, ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid CourseId { get; set; }
    public Guid StudentId { get; set; }
    public DateTime EnrolledOn { get; set; }
    public DateTime? CompletedOn { get; set; }
}
