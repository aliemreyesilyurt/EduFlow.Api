namespace EduFlow.Infrastructure.Logging;

public enum AuditAction
{
    Created,
    Updated,
    Deleted
}

/// <summary>
/// Who changed which row and how. Populated automatically by <see cref="Interceptors.AuditLogInterceptor"/>
/// for every <see cref="Domain.Entities.BaseEntity"/> change on <see cref="Database.ApplicationDbContext"/>,
/// and stored in the separate <see cref="LoggingDbContext"/>.
/// </summary>
public sealed class AuditLog
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? UserId { get; set; }
    public required string EntityName { get; set; }
    public required string EntityId { get; set; }
    public AuditAction Action { get; set; }

    /// <summary>JSON snapshot of the affected properties (old/new values for updates).</summary>
    public string? Changes { get; set; }

    public DateTime Timestamp { get; set; }
}
