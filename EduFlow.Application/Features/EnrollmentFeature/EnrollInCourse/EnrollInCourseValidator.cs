using FluentValidation;

namespace EduFlow.Application.Features.EnrollmentFeature.EnrollInCourse;

public sealed class EnrollInCourseValidator : AbstractValidator<EnrollInCourseRequest>
{
    public EnrollInCourseValidator()
    {
        RuleFor(c => c.CourseId)
            .NotEmpty().WithMessage("CourseId is required");
    }
}
