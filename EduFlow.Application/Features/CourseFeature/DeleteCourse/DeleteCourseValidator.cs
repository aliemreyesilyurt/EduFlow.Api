using FluentValidation;

namespace EduFlow.Application.Features.CourseFeature.DeleteCourse;

public sealed class DeleteCourseValidator : AbstractValidator<DeleteCourseRequest>
{
    public DeleteCourseValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Id is required");
    }
}
