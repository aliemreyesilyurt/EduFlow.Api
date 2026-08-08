using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Features.ExamFeature.UpdateQuestion;

public sealed record UpdateQuestionBody(string Text, int Points, IReadOnlyList<QuestionOptionInput> Options);

internal sealed class UpdateQuestionEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPut("questions/{id:guid}", async (
                Guid id,
                UpdateQuestionBody body,
                IHandler<UpdateQuestionRequest, Result<QuestionResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new UpdateQuestionRequest(id, body.Text, body.Points, body.Options),
                    cancellationToken);

                return result.Match(
                    onSuccess: () => Results.Ok(result.Value),
                    onFailure: error => error.ToHttpResult());
            })
            .WithTags(ApiTags.Exams)
            .RequireAuthorization(PolicyNames.InstructorOrAbove)
            .Produces<QuestionResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }
}
