using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Features.CommentFeature.GetCourseComments;

internal sealed class GetCourseCommentsEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("courses/{courseId:guid}/comments", async (
                Guid courseId,
                IHandler<GetCourseCommentsRequest, Result<GetCourseCommentsResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(new GetCourseCommentsRequest(courseId), cancellationToken);
                return result.Match(
                    onSuccess: () => Results.Ok(result.Value),
                    onFailure: error => error.ToHttpResult());
            })
            .WithTags(ApiTags.Comments)
            .RequireAuthorization(PolicyNames.StudentOrAbove)
            .Produces<GetCourseCommentsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }
}
