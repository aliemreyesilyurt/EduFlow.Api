using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Features.AuthFeature.RefreshToken;

internal sealed class RefreshTokenEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("auth/refresh", async (
                IHandler<RefreshTokenRequest, Result<RefreshTokenResponse>> handler,
                RefreshTokenRequest command,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(command, cancellationToken);
                return result.Match(
                    onSuccess: () => Results.Ok(result.Value),
                    onFailure: error => error.ToHttpResult());
            })
            .WithTags(ApiTags.Auth)
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitPolicies.Auth)
            .Produces<RefreshTokenResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest);
    }
}
