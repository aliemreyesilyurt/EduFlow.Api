using FluentValidation;

namespace EduFlow.Application.Features.CommentFeature.GetStepComments;

public sealed class GetStepCommentsValidator : AbstractValidator<GetStepCommentsRequest>
{
    public GetStepCommentsValidator()
    {
        RuleFor(c => c.StepId)
            .NotEmpty().WithMessage("StepId is required");
    }
}
