using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Enums;

namespace EduFlow.Application.Features.StepFeature.CreateStep;

public sealed record CreateStepBody(string Title, StepContentType ContentType, string? ContentUrl, string? TextContent);

internal sealed class CreateStepEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("courses/{courseId:guid}/steps", async (
                Guid courseId,
                CreateStepBody body,
                IHandler<CreateStepRequest, Result<CreateStepResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new CreateStepRequest(courseId, body.Title, body.ContentType, body.ContentUrl, body.TextContent),
                    cancellationToken);

                return result.Match(
                    onSuccess: () => Results.Ok(result.Value),
                    onFailure: error => error.ToHttpResult());
            })
            .WithTags(ApiTags.Steps)
            .RequireAuthorization(PolicyNames.InstructorOrAbove)
            .Produces<CreateStepResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }
}
