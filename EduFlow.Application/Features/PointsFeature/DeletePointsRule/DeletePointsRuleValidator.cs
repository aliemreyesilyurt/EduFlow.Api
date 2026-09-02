using FluentValidation;

namespace EduFlow.Application.Features.PointsFeature.DeletePointsRule;

public sealed class DeletePointsRuleValidator : AbstractValidator<DeletePointsRuleRequest>
{
    public DeletePointsRuleValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Id is required");
    }
}
