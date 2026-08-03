using FluentValidation;

namespace EduFlow.Application.Features.EnrollmentFeature.CompleteStep;

public sealed class CompleteStepValidator : AbstractValidator<CompleteStepRequest>
{
    public CompleteStepValidator()
    {
        RuleFor(c => c.StepId)
            .NotEmpty().WithMessage("StepId is required");
    }
}
