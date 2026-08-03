using EduFlow.Domain.Abstractions;

namespace EduFlow.Domain.Entities;

public sealed class Rating : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid CourseId { get; set; }
    public Guid StudentId { get; set; }
    public int Value { get; set; }
}
