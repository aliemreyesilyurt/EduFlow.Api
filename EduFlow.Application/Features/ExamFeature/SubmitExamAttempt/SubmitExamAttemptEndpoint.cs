using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Features.ExamFeature.SubmitExamAttempt;

public sealed record SubmitExamAttemptBody(IReadOnlyList<SubmitExamAnswerInput> Answers);

internal sealed class SubmitExamAttemptEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("exam-attempts/{attemptId:guid}/submit", async (
                Guid attemptId,
                SubmitExamAttemptBody body,
                IHandler<SubmitExamAttemptRequest, Result<ExamAttemptResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new SubmitExamAttemptRequest(attemptId, body.Answers), cancellationToken);

                return result.Match(
                    onSuccess: () => Results.Ok(result.Value),
                    onFailure: error => error.ToHttpResult());
            })
            .WithTags(ApiTags.Exams)
            .RequireAuthorization(PolicyNames.StudentOnly)
            .Produces<ExamAttemptResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
    }
}
