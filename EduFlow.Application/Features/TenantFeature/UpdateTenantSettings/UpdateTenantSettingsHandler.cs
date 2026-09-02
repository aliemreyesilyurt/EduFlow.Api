namespace EduFlow.Application.Features.TenantFeature.UpdateTenantSettings;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.AuthFeature;
using EduFlow.Application.Features.TenantFeature.GetTenantSettings;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record UpdateTenantSettingsRequest(
    bool AllowSelfRegistration,
    string? ProctoringConsentText,
    int ProctoringRetentionDays);

public sealed class UpdateTenantSettingsHandler(
    IRepository<Tenant> tenantRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<UpdateTenantSettingsRequest, Result<GetTenantSettingsResponse>>
{
    public async Task<Result<GetTenantSettingsResponse>> HandleAsync(UpdateTenantSettingsRequest command, CancellationToken cancellationToken)
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

        tenant.AllowSelfRegistration = command.AllowSelfRegistration;
        tenant.ProctoringConsentText = command.ProctoringConsentText;
        tenant.ProctoringRetentionDays = command.ProctoringRetentionDays;

        await tenantRepository.UpdateAsync(tenant, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new GetTenantSettingsResponse(
            tenant.Id, tenant.Name, tenant.Slug, tenant.AllowSelfRegistration,
            tenant.ProctoringConsentText, tenant.ProctoringRetentionDays));
    }
}
