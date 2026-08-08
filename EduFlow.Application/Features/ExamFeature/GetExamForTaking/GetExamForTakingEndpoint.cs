using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Features.ExamFeature.GetExamForTaking;

internal sealed class GetExamForTakingEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("courses/{courseId:guid}/exam/take", async (
                Guid courseId,
                IHandler<GetExamForTakingRequest, Result<ExamTakingResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(new GetExamForTakingRequest(courseId), cancellationToken);
                return result.Match(
                    onSuccess: () => Results.Ok(result.Value),
                    onFailure: error => error.ToHttpResult());
            })
            .WithTags(ApiTags.Exams)
            .RequireAuthorization(PolicyNames.StudentOnly)
            .Produces<ExamTakingResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }
}
