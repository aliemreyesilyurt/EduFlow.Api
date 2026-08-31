using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Features.TenantFeature.CreateTenant;

internal sealed class CreateTenantEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("tenants", async (
                CreateTenantRequest command,
                IHandler<CreateTenantRequest, Result<CreateTenantResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(command, cancellationToken);
                return result.Match(
                    onSuccess: () => Results.Created($"tenants/{result.Value.TenantId}", result.Value),
                    onFailure: error => error.ToHttpResult());
            })
            .WithTags(ApiTags.Tenants)
            .RequireAuthorization(PolicyNames.SysAdminOnly)
            .Produces<CreateTenantResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }
}
