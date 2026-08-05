using FluentValidation;

namespace EduFlow.Application.Features.StepFeature.GetStepContent;

public sealed class GetStepContentValidator : AbstractValidator<GetStepContentRequest>
{
    public GetStepContentValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Id is required");
    }
}
