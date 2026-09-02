using FluentValidation;

namespace EduFlow.Application.Features.PointsFeature.UpdatePointsRule;

public sealed class UpdatePointsRuleValidator : AbstractValidator<UpdatePointsRuleRequest>
{
    public UpdatePointsRuleValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Id is required");

        RuleFor(c => c.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(c => c.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

        RuleFor(c => c.PointsCost)
            .GreaterThan(0).WithMessage("PointsCost must be positive");
    }
}
