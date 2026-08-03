namespace EduFlow.Application.Features.SystemSettingsFeature.UpsertSystemSetting;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Settings;
using EduFlow.Domain.Abstractions;

public sealed record UpsertSystemSettingRequest(string Key, string? Value);

public sealed class UpsertSystemSettingHandler(
    ISystemSettingsService settingsService,
    ITenantContext tenantContext) : IHandler<UpsertSystemSettingRequest, Result>
{
    public Task<Result> HandleAsync(UpsertSystemSettingRequest command, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.IsSysAdmin ? null : tenantContext.TenantId;

        return settingsService.UpsertAsync(command.Key, command.Value, tenantId, cancellationToken);
    }
}
