using FluentValidation;

namespace EduFlow.Application.Features.StepFeature.ReorderSteps;

public sealed class ReorderStepsValidator : AbstractValidator<ReorderStepsRequest>
{
    public ReorderStepsValidator()
    {
        RuleFor(c => c.CourseId)
            .NotEmpty().WithMessage("CourseId is required");

        RuleFor(c => c.StepIds)
            .NotEmpty().WithMessage("At least one step id is required")
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Step ids must not contain duplicates");
    }
}
