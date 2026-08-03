using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Enums;

namespace EduFlow.Domain.Entities;

public sealed class Step : AuditableEntity, ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid CourseId { get; set; }
    public required string Title { get; set; }
    public int Order { get; set; }
    public StepContentType ContentType { get; set; }
    public string? ContentUrl { get; set; }
    public string? TextContent { get; set; }
}
