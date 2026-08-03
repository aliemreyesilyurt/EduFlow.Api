using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Features.AuthFeature.ForgotPassword;

internal sealed class ForgotPasswordEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("auth/password/forgot", async (
                IHandler<ForgotPasswordRequest, Result> handler,
                ForgotPasswordRequest command,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(command, cancellationToken);
                return result.Match(
                    onSuccess: () => Results.NoContent(),
                    onFailure: error => error.ToHttpResult());
            })
            .WithTags(ApiTags.Auth)
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitPolicies.Auth)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest);
    }
}
