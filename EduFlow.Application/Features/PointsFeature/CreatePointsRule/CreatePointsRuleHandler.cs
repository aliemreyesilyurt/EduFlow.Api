namespace EduFlow.Application.Features.PointsFeature.CreatePointsRule;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record CreatePointsRuleRequest(string Title, string? Description, int PointsCost, bool IsActive);

public sealed class CreatePointsRuleHandler(
    IRepository<PointsRule> ruleRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<CreatePointsRuleRequest, Result<PointsRuleResponse>>
{
    public async Task<Result<PointsRuleResponse>> HandleAsync(CreatePointsRuleRequest command, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null)
        {
            return PointsErrors.TenantRequired;
        }

        var rule = new PointsRule
        {
            Id = Guid.CreateVersion7(),
            Title = command.Title,
            Description = command.Description,
            PointsCost = command.PointsCost,
            IsActive = command.IsActive
        };

        await ruleRepository.AddAsync(rule, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new PointsRuleResponse(rule.Id, rule.Title, rule.Description, rule.PointsCost, rule.IsActive));
    }
}
