using FluentValidation;

namespace EduFlow.Application.Features.CourseFeature.UpdateCourse;

public sealed class UpdateCourseValidator : AbstractValidator<UpdateCourseRequest>
{
    public UpdateCourseValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Id is required");

        RuleFor(c => c.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(c => c.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");
    }
}
