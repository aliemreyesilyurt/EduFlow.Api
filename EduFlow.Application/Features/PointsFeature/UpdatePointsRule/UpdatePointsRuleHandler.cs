namespace EduFlow.Application.Features.PointsFeature.UpdatePointsRule;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record UpdatePointsRuleRequest(Guid Id, string Title, string? Description, int PointsCost, bool IsActive);

public sealed class UpdatePointsRuleHandler(
    IRepository<PointsRule> ruleRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<UpdatePointsRuleRequest, Result<PointsRuleResponse>>
{
    public async Task<Result<PointsRuleResponse>> HandleAsync(UpdatePointsRuleRequest command, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
        {
            return PointsErrors.TenantRequired;
        }

        var rule = await ruleRepository.FindAsync(r => r.Id == command.Id, cancellationToken);

        if (rule is null || rule.TenantId != tenantId)
        {
            return PointsErrors.RuleNotFound(command.Id);
        }

        rule.Title = command.Title;
        rule.Description = command.Description;
        rule.PointsCost = command.PointsCost;
        rule.IsActive = command.IsActive;

        await ruleRepository.UpdateAsync(rule, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new PointsRuleResponse(rule.Id, rule.Title, rule.Description, rule.PointsCost, rule.IsActive));
    }
}
