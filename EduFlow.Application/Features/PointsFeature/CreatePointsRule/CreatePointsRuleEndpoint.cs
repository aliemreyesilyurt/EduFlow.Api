using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Features.PointsFeature.CreatePointsRule;

public sealed record CreatePointsRuleBody(string Title, string? Description, int PointsCost, bool IsActive);

internal sealed class CreatePointsRuleEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("tenants/points-rules", async (
                CreatePointsRuleBody body,
                IHandler<CreatePointsRuleRequest, Result<PointsRuleResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new CreatePointsRuleRequest(body.Title, body.Description, body.PointsCost, body.IsActive),
                    cancellationToken);

                return result.Match(
                    onSuccess: () => Results.Ok(result.Value),
                    onFailure: error => error.ToHttpResult());
            })
            .WithTags(ApiTags.Points)
            .RequireAuthorization(PolicyNames.TenantAdminOrAbove)
            .Produces<PointsRuleResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }
}
