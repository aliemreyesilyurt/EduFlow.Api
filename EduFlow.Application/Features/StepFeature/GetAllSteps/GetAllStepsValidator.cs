using FluentValidation;

namespace EduFlow.Application.Features.StepFeature.GetAllSteps;

public sealed class GetAllStepsValidator : AbstractValidator<GetAllStepsRequest>
{
    public GetAllStepsValidator()
    {
        RuleFor(c => c.CourseId)
            .NotEmpty().WithMessage("CourseId is required");
    }
}
