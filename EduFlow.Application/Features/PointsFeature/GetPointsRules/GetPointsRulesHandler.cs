namespace EduFlow.Application.Features.PointsFeature.GetPointsRules;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record GetPointsRulesRequest;

public sealed record GetPointsRulesResponse(IReadOnlyList<PointsRuleResponse> Rules);

public sealed class GetPointsRulesHandler(
    IRepository<PointsRule> ruleRepository) : IHandler<GetPointsRulesRequest, Result<GetPointsRulesResponse>>
{
    public async Task<Result<GetPointsRulesResponse>> HandleAsync(GetPointsRulesRequest command, CancellationToken cancellationToken)
    {
        var rules = (await ruleRepository.GetAllAsync(cancellationToken))
            .Where(r => r.IsActive)
            .OrderBy(r => r.PointsCost)
            .Select(r => new PointsRuleResponse(r.Id, r.Title, r.Description, r.PointsCost, r.IsActive))
            .ToList();

        return Result.Success(new GetPointsRulesResponse(rules));
    }
}
