using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Abstractions.Errors;

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
                    onFailure: error => error.Type == ErrorType.Unauthorized
                        ? Results.Unauthorized()
                        : Results.BadRequest(error));
            })
            .WithTags(ApiTags.Auth)
            .AllowAnonymous()
            .Produces<RefreshTokenResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest);
    }
}
