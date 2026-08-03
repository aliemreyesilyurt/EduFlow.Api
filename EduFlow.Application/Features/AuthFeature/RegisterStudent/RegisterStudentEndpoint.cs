using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Features.AuthFeature.RegisterStudent;

internal sealed class RegisterStudentEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("auth/register/student", async (
                IHandler<RegisterStudentRequest, Result<RegisterStudentResponse>> handler,
                RegisterStudentRequest command,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(command, cancellationToken);
                return result.Match(
                    onSuccess: () => Results.Accepted(value: result.Value),
                    onFailure: error => error.ToHttpResult());
            })
            .WithTags(ApiTags.Auth)
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitPolicies.Auth)
            .Produces<RegisterStudentResponse>(StatusCodes.Status202Accepted)
            .Produces(StatusCodes.Status400BadRequest);
    }
}
