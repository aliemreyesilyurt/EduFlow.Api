using FluentValidation;

namespace EduFlow.Application.Features.CommentFeature.CreateStepComment;

public sealed class CreateStepCommentValidator : AbstractValidator<CreateStepCommentRequest>
{
    public CreateStepCommentValidator()
    {
        RuleFor(c => c.StepId)
            .NotEmpty().WithMessage("StepId is required");

        RuleFor(c => c.Content)
            .NotEmpty().WithMessage("Content is required")
            .MaximumLength(2000).WithMessage("Content must not exceed 2000 characters");
    }
}
