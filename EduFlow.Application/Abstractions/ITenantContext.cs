namespace EduFlow.Application.Abstractions;

public interface ITenantContext
{
    Guid? TenantId { get; }
    Guid? UserId { get; }
    bool IsSysAdmin { get; }
    bool IsInRole(string role);
}
