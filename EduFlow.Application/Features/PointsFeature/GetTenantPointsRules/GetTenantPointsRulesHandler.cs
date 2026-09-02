namespace EduFlow.Application.Features.PointsFeature.GetTenantPointsRules;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record GetTenantPointsRulesRequest;

public sealed record GetTenantPointsRulesResponse(IReadOnlyList<PointsRuleResponse> Rules);

public sealed class GetTenantPointsRulesHandler(
    IRepository<PointsRule> ruleRepository,
    ITenantContext tenantContext) : IHandler<GetTenantPointsRulesRequest, Result<GetTenantPointsRulesResponse>>
{
    public async Task<Result<GetTenantPointsRulesResponse>> HandleAsync(GetTenantPointsRulesRequest command, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
        {
            return PointsErrors.TenantRequired;
        }

        var rules = (await ruleRepository.GetAllAsync(cancellationToken))
            .Where(r => r.TenantId == tenantId)
            .OrderBy(r => r.PointsCost)
            .Select(r => new PointsRuleResponse(r.Id, r.Title, r.Description, r.PointsCost, r.IsActive))
            .ToList();

        return Result.Success(new GetTenantPointsRulesResponse(rules));
    }
}
