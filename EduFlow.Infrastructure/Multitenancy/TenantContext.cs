using System.Security.Claims;
using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using Microsoft.AspNetCore.Http;

namespace EduFlow.Infrastructure.Multitenancy;

public sealed class TenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? TenantId => GetGuidClaim("tenant_id");

    public Guid? UserId => GetGuidClaim(ClaimTypes.NameIdentifier);

    public bool IsSysAdmin => User?.IsInRole(Roles.SysAdmin) ?? false;

    private Guid? GetGuidClaim(string claimType)
    {
        var value = User?.FindFirst(claimType)?.Value;
        return Guid.TryParse(value, out var guid) ? guid : null;
    }
}
