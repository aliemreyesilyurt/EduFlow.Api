using FluentValidation;

namespace EduFlow.Application.Features.CommentFeature.CreateCourseComment;

public sealed class CreateCourseCommentValidator : AbstractValidator<CreateCourseCommentRequest>
{
    public CreateCourseCommentValidator()
    {
        RuleFor(c => c.CourseId)
            .NotEmpty().WithMessage("CourseId is required");

        RuleFor(c => c.Content)
            .NotEmpty().WithMessage("Content is required")
            .MaximumLength(2000).WithMessage("Content must not exceed 2000 characters");
    }
}
