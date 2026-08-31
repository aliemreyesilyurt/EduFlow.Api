using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Features.TenantFeature.GetTenantSettings;

internal sealed class GetTenantSettingsEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("tenants/settings", async (
                IHandler<GetTenantSettingsRequest, Result<GetTenantSettingsResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(new GetTenantSettingsRequest(), cancellationToken);
                return result.Match(
                    onSuccess: () => Results.Ok(result.Value),
                    onFailure: error => error.ToHttpResult());
            })
            .WithTags(ApiTags.Tenants)
            .RequireAuthorization(PolicyNames.TenantAdminOrAbove)
            .Produces<GetTenantSettingsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }
}
