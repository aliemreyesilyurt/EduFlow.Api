using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Features.CommentFeature.CreateCourseComment;

internal sealed class CreateCourseCommentEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("courses/{courseId:guid}/comments", async (
                Guid courseId,
                CreateCourseCommentBody body,
                IHandler<CreateCourseCommentRequest, Result<CommentResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new CreateCourseCommentRequest(courseId, body.Content), cancellationToken);
                return result.Match(
                    onSuccess: () => Results.Ok(result.Value),
                    onFailure: error => error.ToHttpResult());
            })
            .WithTags(ApiTags.Comments)
            .RequireAuthorization(PolicyNames.StudentOnly)
            .Produces<CommentResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }
}

public sealed record CreateCourseCommentBody(string Content);
