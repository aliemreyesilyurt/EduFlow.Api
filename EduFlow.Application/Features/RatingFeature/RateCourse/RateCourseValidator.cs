using FluentValidation;

namespace EduFlow.Application.Features.RatingFeature.RateCourse;

public sealed class RateCourseValidator : AbstractValidator<RateCourseRequest>
{
    public RateCourseValidator()
    {
        RuleFor(c => c.CourseId)
            .NotEmpty().WithMessage("CourseId is required");

        RuleFor(c => c.Value)
            .InclusiveBetween(1, 5).WithMessage("Value must be between 1 and 5");
    }
}
