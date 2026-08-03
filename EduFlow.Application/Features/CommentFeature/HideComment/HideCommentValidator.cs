using FluentValidation;

namespace EduFlow.Application.Features.CommentFeature.HideComment;

public sealed class HideCommentValidator : AbstractValidator<HideCommentRequest>
{
    public HideCommentValidator()
    {
        RuleFor(c => c.CommentId)
            .NotEmpty().WithMessage("CommentId is required");
    }
}
