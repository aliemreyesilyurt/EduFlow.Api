using FluentValidation;

namespace EduFlow.Application.Features.CommentFeature.GetCourseComments;

public sealed class GetCourseCommentsValidator : AbstractValidator<GetCourseCommentsRequest>
{
    public GetCourseCommentsValidator()
    {
        RuleFor(c => c.CourseId)
            .NotEmpty().WithMessage("CourseId is required");
    }
}
