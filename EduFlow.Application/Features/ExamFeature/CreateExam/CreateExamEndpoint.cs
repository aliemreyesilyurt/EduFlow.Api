using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Features.ExamFeature.CreateExam;

public sealed record CreateExamBody(
    string Title,
    int PassScorePercentage,
    int? TimeLimitMinutes,
    int? MaxAttempts,
    bool ProctoringEnabled,
    bool RequireCamera,
    int? SnapshotIntervalSeconds,
    int? ViolationWarningThreshold,
    int RewardPoints);

internal sealed class CreateExamEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("courses/{courseId:guid}/exam", async (
                Guid courseId,
                CreateExamBody body,
                IHandler<CreateExamRequest, Result<ExamSummaryResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new CreateExamRequest(
                        courseId, body.Title, body.PassScorePercentage, body.TimeLimitMinutes, body.MaxAttempts,
                        body.ProctoringEnabled, body.RequireCamera, body.SnapshotIntervalSeconds, body.ViolationWarningThreshold,
                        body.RewardPoints),
                    cancellationToken);

                return result.Match(
                    onSuccess: () => Results.Ok(result.Value),
                    onFailure: error => error.ToHttpResult());
            })
            .WithTags(ApiTags.Exams)
            .RequireAuthorization(PolicyNames.InstructorOrAbove)
            .Produces<ExamSummaryResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
    }
}
