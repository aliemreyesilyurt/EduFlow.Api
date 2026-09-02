namespace EduFlow.Application.Features.PointsFeature.DeletePointsRule;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record DeletePointsRuleRequest(Guid Id);

public sealed class DeletePointsRuleHandler(
    IRepository<PointsRule> ruleRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<DeletePointsRuleRequest, Result>
{
    public async Task<Result> HandleAsync(DeletePointsRuleRequest command, CancellationToken cancellationToken)
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

        await ruleRepository.DeleteAsync(rule, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
