using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Application.Features.TenantFeature.GetTenantSettings;
using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Features.TenantFeature.UpdateTenantSettings;

public sealed record UpdateTenantSettingsBody(bool AllowSelfRegistration);

internal sealed class UpdateTenantSettingsEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPut("tenants/settings", async (
                UpdateTenantSettingsBody body,
                IHandler<UpdateTenantSettingsRequest, Result<GetTenantSettingsResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new UpdateTenantSettingsRequest(body.AllowSelfRegistration),
                    cancellationToken);

                return result.Match(
                    onSuccess: () => Results.Ok(result.Value),
                    onFailure: error => error.ToHttpResult());
            })
            .WithTags(ApiTags.Tenants)
            .RequireAuthorization(PolicyNames.TenantAdminOrAbove)
            .Produces<GetTenantSettingsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }
}
