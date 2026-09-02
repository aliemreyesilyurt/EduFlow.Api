using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Features.ProctoringFeature.GetProctoringSnapshot;

internal sealed class GetProctoringSnapshotEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("exam-attempts/{attemptId:guid}/proctoring/snapshots/{snapshotId:guid}", async (
                Guid attemptId,
                Guid snapshotId,
                IHandler<GetProctoringSnapshotRequest, Result<GetProctoringSnapshotResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new GetProctoringSnapshotRequest(attemptId, snapshotId), cancellationToken);

                return result.Match(
                    onSuccess: () => Results.Stream(result.Value.Content, result.Value.ContentType, result.Value.FileName),
                    onFailure: error => error.ToHttpResult());
            })
            .WithTags(ApiTags.Proctoring)
            .RequireAuthorization(PolicyNames.InstructorOrAbove)
            .Produces(StatusCodes.Status200OK, contentType: "application/octet-stream")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }
}
