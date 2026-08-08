using FluentValidation;

namespace EduFlow.Application.Features.ExamFeature.StartExamAttempt;

public sealed class StartExamAttemptValidator : AbstractValidator<StartExamAttemptRequest>
{
    public StartExamAttemptValidator()
    {
        RuleFor(c => c.CourseId)
            .NotEmpty().WithMessage("CourseId is required");
    }
}
