using FluentValidation;

namespace EduFlow.Application.Features.ExamFeature.CreateExam;

public sealed class CreateExamValidator : AbstractValidator<CreateExamRequest>
{
    public CreateExamValidator()
    {
        RuleFor(c => c.CourseId)
            .NotEmpty().WithMessage("CourseId is required");

        RuleFor(c => c.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(c => c.PassScorePercentage)
            .InclusiveBetween(1, 100).WithMessage("PassScorePercentage must be between 1 and 100");

        RuleFor(c => c.TimeLimitMinutes)
            .GreaterThan(0).WithMessage("TimeLimitMinutes must be positive")
            .When(c => c.TimeLimitMinutes is not null);

        RuleFor(c => c.MaxAttempts)
            .GreaterThan(0).WithMessage("MaxAttempts must be positive")
            .When(c => c.MaxAttempts is not null);
    }
}
