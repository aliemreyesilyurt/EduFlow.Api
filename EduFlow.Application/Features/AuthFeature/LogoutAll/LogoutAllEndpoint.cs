using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Features.AuthFeature.LogoutAll;

internal sealed class LogoutAllEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("auth/logout-all", async (
                IHandler<LogoutAllRequest, Result> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(new LogoutAllRequest(), cancellationToken);
                return result.Match(
                    onSuccess: () => Results.NoContent(),
                    onFailure: error => error.ToHttpResult());
            })
            .WithTags(ApiTags.Auth)
            .RequireAuthorization(PolicyNames.Authenticated)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }
}
