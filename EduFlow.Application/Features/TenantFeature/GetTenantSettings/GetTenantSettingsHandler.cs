namespace EduFlow.Application.Features.TenantFeature.GetTenantSettings;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.AuthFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record GetTenantSettingsRequest;

public sealed record GetTenantSettingsResponse(Guid TenantId, string Name, string Slug, bool AllowSelfRegistration);

public sealed class GetTenantSettingsHandler(
    IRepository<Tenant> tenantRepository,
    ITenantContext tenantContext) : IHandler<GetTenantSettingsRequest, Result<GetTenantSettingsResponse>>
{
    public async Task<Result<GetTenantSettingsResponse>> HandleAsync(GetTenantSettingsRequest command, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
        {
            return AuthErrors.TenantNotFound;
        }

        var tenant = await tenantRepository.FindAsync(t => t.Id == tenantId, cancellationToken);

        if (tenant is null)
        {
            return AuthErrors.TenantNotFound;
        }

        return Result.Success(new GetTenantSettingsResponse(tenant.Id, tenant.Name, tenant.Slug, tenant.AllowSelfRegistration));
    }
}
