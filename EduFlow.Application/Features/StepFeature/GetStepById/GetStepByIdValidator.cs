using FluentValidation;

namespace EduFlow.Application.Features.StepFeature.GetStepById;

public sealed class GetStepByIdValidator : AbstractValidator<GetStepByIdRequest>
{
    public GetStepByIdValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Id is required");
    }
}
