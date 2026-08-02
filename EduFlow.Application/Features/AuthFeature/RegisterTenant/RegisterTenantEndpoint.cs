using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Features.AuthFeature.RegisterTenant;

internal sealed class RegisterTenantEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("auth/register/tenant", async (
                IHandler<RegisterTenantRequest, Result<RegisterTenantResponse>> handler,
                RegisterTenantRequest command,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(command, cancellationToken);
                return result.Match(
                    onSuccess: () => Results.Ok(result.Value),
                    onFailure: error => Results.BadRequest(error));
            })
            .WithTags(ApiTags.Auth)
            .AllowAnonymous()
            .Produces<RegisterTenantResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }
}
