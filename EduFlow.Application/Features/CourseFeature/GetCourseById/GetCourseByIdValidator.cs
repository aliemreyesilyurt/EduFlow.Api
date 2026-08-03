using FluentValidation;

namespace EduFlow.Application.Features.CourseFeature.GetCourseById;

public sealed class GetCourseByIdValidator : AbstractValidator<GetCourseByIdRequest>
{
    public GetCourseByIdValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Id is required");
    }
}
