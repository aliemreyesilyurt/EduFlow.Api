using FluentValidation;

namespace EduFlow.Application.Features.StepFeature.DeleteStep;

public sealed class DeleteStepValidator : AbstractValidator<DeleteStepRequest>
{
    public DeleteStepValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Id is required");
    }
}
