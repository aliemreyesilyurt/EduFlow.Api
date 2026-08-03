using FluentValidation;

namespace EduFlow.Application.Features.EnrollmentFeature.GetEnrolledStudents;

public sealed class GetEnrolledStudentsValidator : AbstractValidator<GetEnrolledStudentsRequest>
{
    public GetEnrolledStudentsValidator()
    {
        RuleFor(c => c.CourseId)
            .NotEmpty().WithMessage("CourseId is required");
    }
}
