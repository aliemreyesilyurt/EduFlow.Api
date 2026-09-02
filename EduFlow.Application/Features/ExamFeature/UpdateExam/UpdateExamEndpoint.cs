using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Features.ExamFeature.UpdateExam;

public sealed record UpdateExamBody(
    string Title,
    int PassScorePercentage,
    int? TimeLimitMinutes,
    int? MaxAttempts,
    bool ProctoringEnabled,
    bool RequireCamera,
    int? SnapshotIntervalSeconds,
    int? ViolationWarningThreshold);

internal sealed class UpdateExamEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPut("exams/{id:guid}", async (
                Guid id,
                UpdateExamBody body,
                IHandler<UpdateExamRequest, Result<ExamSummaryResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new UpdateExamRequest(
                        id, body.Title, body.PassScorePercentage, body.TimeLimitMinutes, body.MaxAttempts,
                        body.ProctoringEnabled, body.RequireCamera, body.SnapshotIntervalSeconds, body.ViolationWarningThreshold),
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
            .Produces(StatusCodes.Status404NotFound);
    }
}
