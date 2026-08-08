using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Features.ExamFeature.GetExamAttempt;

internal sealed class GetExamAttemptEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("exam-attempts/{id:guid}", async (
                Guid id,
                IHandler<GetExamAttemptRequest, Result<ExamAttemptResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(new GetExamAttemptRequest(id), cancellationToken);
                return result.Match(
                    onSuccess: () => Results.Ok(result.Value),
                    onFailure: error => error.ToHttpResult());
            })
            .WithTags(ApiTags.Exams)
            .RequireAuthorization(PolicyNames.StudentOrAbove)
            .Produces<ExamAttemptResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }
}
