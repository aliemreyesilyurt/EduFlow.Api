using FluentValidation;

namespace EduFlow.Application.Features.CourseFeature.ArchiveCourse;

public sealed class ArchiveCourseValidator : AbstractValidator<ArchiveCourseRequest>
{
    public ArchiveCourseValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Id is required");
    }
}
