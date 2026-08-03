using FluentValidation;

namespace EduFlow.Application.Features.EnrollmentFeature.Unenroll;

public sealed class UnenrollValidator : AbstractValidator<UnenrollRequest>
{
    public UnenrollValidator()
    {
        RuleFor(c => c.CourseId)
            .NotEmpty().WithMessage("CourseId is required");
    }
}
