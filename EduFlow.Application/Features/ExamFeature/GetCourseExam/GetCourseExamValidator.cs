using FluentValidation;

namespace EduFlow.Application.Features.ExamFeature.GetCourseExam;

public sealed class GetCourseExamValidator : AbstractValidator<GetCourseExamRequest>
{
    public GetCourseExamValidator()
    {
        RuleFor(c => c.CourseId)
            .NotEmpty().WithMessage("CourseId is required");
    }
}
