using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Features.AuthFeature.ResendConfirmationEmail;

internal sealed class ResendConfirmationEmailEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("auth/confirm-email/resend", async (
                IHandler<ResendConfirmationEmailRequest, Result> handler,
                ResendConfirmationEmailRequest command,
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
