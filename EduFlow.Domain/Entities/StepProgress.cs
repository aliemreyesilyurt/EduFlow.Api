using EduFlow.Domain.Abstractions;

namespace EduFlow.Domain.Entities;

public sealed class StepProgress : AuditableEntity, ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EnrollmentId { get; set; }
    public Guid StepId { get; set; }
    public DateTime CompletedOn { get; set; }
}
