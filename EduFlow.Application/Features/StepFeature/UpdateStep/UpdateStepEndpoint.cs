using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Enums;

namespace EduFlow.Application.Features.StepFeature.UpdateStep;

public sealed record UpdateStepBody(string Title, StepContentType ContentType, string? ContentUrl, string? TextContent);

internal sealed class UpdateStepEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPut("steps/{id:guid}", async (
                Guid id,
                UpdateStepBody body,
                IHandler<UpdateStepRequest, Result<UpdateStepResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new UpdateStepRequest(id, body.Title, body.ContentType, body.ContentUrl, body.TextContent),
                    cancellationToken);

                return result.Match(
                    onSuccess: () => Results.Ok(result.Value),
                    onFailure: error => error.ToHttpResult());
            })
            .WithTags(ApiTags.Steps)
            .RequireAuthorization(PolicyNames.InstructorOrAbove)
            .Produces<UpdateStepResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }
}
