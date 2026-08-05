using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Features.StepFeature.GetStepContent;

internal sealed class GetStepContentEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("steps/{id:guid}/content", async (
                Guid id,
                IHandler<GetStepContentRequest, Result<GetStepContentResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(new GetStepContentRequest(id), cancellationToken);
                return result.Match(
                    onSuccess: () => Results.Stream(result.Value.Content, result.Value.ContentType, result.Value.FileName),
                    onFailure: error => error.ToHttpResult());
            })
            .WithTags(ApiTags.Steps)
            .RequireAuthorization(PolicyNames.StudentOrAbove)
            .Produces(StatusCodes.Status200OK, contentType: "application/octet-stream")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }
}
