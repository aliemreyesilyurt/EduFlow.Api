namespace EduFlow.Domain.Entities;

/// <summary>
/// Key/value configuration entry. <see cref="TenantId"/> is null for a global/system default;
/// a non-null value is a per-tenant override resolved on top of the global default.
/// </summary>
public sealed class SystemSetting : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public required string Key { get; set; }
    public string? Value { get; set; }
    public bool IsSecret { get; set; }
    public string? Description { get; set; }
}
