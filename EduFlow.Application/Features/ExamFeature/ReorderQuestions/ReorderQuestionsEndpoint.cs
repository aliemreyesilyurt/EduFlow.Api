using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Features.ExamFeature.ReorderQuestions;

public sealed record ReorderQuestionsBody(IReadOnlyList<Guid> QuestionIds);

internal sealed class ReorderQuestionsEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("exams/{examId:guid}/questions/reorder", async (
                Guid examId,
                ReorderQuestionsBody body,
                IHandler<ReorderQuestionsRequest, Result> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(new ReorderQuestionsRequest(examId, body.QuestionIds), cancellationToken);
                return result.Match(
                    onSuccess: () => Results.NoContent(),
                    onFailure: error => error.ToHttpResult());
            })
            .WithTags(ApiTags.Exams)
            .RequireAuthorization(PolicyNames.InstructorOrAbove)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }
}
