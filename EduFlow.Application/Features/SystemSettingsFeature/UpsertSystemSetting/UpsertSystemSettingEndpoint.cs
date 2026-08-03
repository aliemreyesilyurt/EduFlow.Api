using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Features.SystemSettingsFeature.UpsertSystemSetting;

public sealed record UpsertSystemSettingBody(string? Value);

internal sealed class UpsertSystemSettingEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPut("system-settings/{key}", async (
                string key,
                UpsertSystemSettingBody body,
                IHandler<UpsertSystemSettingRequest, Result> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(new UpsertSystemSettingRequest(key, body.Value), cancellationToken);
                return result.Match(
                    onSuccess: () => Results.NoContent(),
                    onFailure: error => error.ToHttpResult());
            })
            .WithTags(ApiTags.SystemSettings)
            .RequireAuthorization(PolicyNames.TenantAdminOrAbove)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }
}
