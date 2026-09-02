using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Features.ProctoringFeature.ReviewExamAttempt;

public sealed record ReviewExamAttemptBody(bool Approved, string? Note);

internal sealed class ReviewExamAttemptEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("exam-attempts/{id:guid}/review", async (
                Guid id,
                ReviewExamAttemptBody body,
                IHandler<ReviewExamAttemptRequest, Result<ReviewExamAttemptResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new ReviewExamAttemptRequest(id, body.Approved, body.Note), cancellationToken);

                return result.Match(
                    onSuccess: () => Results.Ok(result.Value),
                    onFailure: error => error.ToHttpResult());
            })
            .WithTags(ApiTags.Proctoring)
            .RequireAuthorization(PolicyNames.InstructorOrAbove)
            .Produces<ReviewExamAttemptResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
    }
}
