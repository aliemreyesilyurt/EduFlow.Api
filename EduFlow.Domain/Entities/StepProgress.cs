using EduFlow.Domain.Abstractions;

namespace EduFlow.Domain.Entities;

public sealed class StepProgress : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid EnrollmentId { get; set; }
    public Guid StepId { get; set; }
    public DateTime CompletedOn { get; set; }
}
