using FluentValidation;

namespace EduFlow.Application.Features.CourseFeature.CreateCourse;

public sealed class CreateCourseValidator : AbstractValidator<CreateCourseRequest>
{
    public CreateCourseValidator()
    {
        RuleFor(c => c.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(c => c.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");
    }
}
