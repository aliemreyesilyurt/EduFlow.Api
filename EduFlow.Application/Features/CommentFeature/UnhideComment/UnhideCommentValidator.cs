using FluentValidation;

namespace EduFlow.Application.Features.CommentFeature.UnhideComment;

public sealed class UnhideCommentValidator : AbstractValidator<UnhideCommentRequest>
{
    public UnhideCommentValidator()
    {
        RuleFor(c => c.CommentId)
            .NotEmpty().WithMessage("CommentId is required");
    }
}
