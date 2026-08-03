using FluentValidation;

namespace EduFlow.Application.Features.CourseFeature.PublishCourse;

public sealed class PublishCourseValidator : AbstractValidator<PublishCourseRequest>
{
    public PublishCourseValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Id is required");
    }
}
