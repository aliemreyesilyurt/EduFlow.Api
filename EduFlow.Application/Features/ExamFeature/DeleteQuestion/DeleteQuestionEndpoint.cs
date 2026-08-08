using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Features.ExamFeature.DeleteQuestion;

internal sealed class DeleteQuestionEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapDelete("questions/{id:guid}", async (
                Guid id,
                IHandler<DeleteQuestionRequest, Result> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(new DeleteQuestionRequest(id), cancellationToken);
                return result.Match(
                    onSuccess: () => Results.NoContent(),
                    onFailure: error => error.ToHttpResult());
            })
            .WithTags(ApiTags.Exams)
            .RequireAuthorization(PolicyNames.InstructorOrAbove)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }
}
