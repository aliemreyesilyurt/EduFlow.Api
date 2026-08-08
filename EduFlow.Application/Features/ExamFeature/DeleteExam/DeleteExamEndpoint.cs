using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Features.ExamFeature.DeleteExam;

internal sealed class DeleteExamEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapDelete("exams/{id:guid}", async (
                Guid id,
                IHandler<DeleteExamRequest, Result> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(new DeleteExamRequest(id), cancellationToken);
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
