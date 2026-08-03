namespace EduFlow.Application.Features.SystemSettingsFeature.GetSystemSettings;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Settings;
using EduFlow.Domain.Abstractions;

public sealed record GetSystemSettingsRequest;

public sealed record GetSystemSettingsResponse(IReadOnlyList<SystemSettingItem> Settings);

public sealed class GetSystemSettingsHandler(
    ISystemSettingsService settingsService,
    ITenantContext tenantContext) : IHandler<GetSystemSettingsRequest, Result<GetSystemSettingsResponse>>
{
    public async Task<Result<GetSystemSettingsResponse>> HandleAsync(GetSystemSettingsRequest command, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.IsSysAdmin ? null : tenantContext.TenantId;
        var settings = await settingsService.GetAllAsync(tenantId, cancellationToken);

        return Result.Success(new GetSystemSettingsResponse(settings));
    }
}
