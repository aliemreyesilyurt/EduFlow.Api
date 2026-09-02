using FluentValidation;

namespace EduFlow.Application.Features.ExamFeature.UpdateExam;

public sealed class UpdateExamValidator : AbstractValidator<UpdateExamRequest>
{
    public UpdateExamValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Id is required");

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

        RuleFor(c => c.ProctoringEnabled)
            .Equal(true).WithMessage("RequireCamera cannot be enabled without ProctoringEnabled")
            .When(c => c.RequireCamera);

        RuleFor(c => c.SnapshotIntervalSeconds)
            .InclusiveBetween(15, 600).WithMessage("SnapshotIntervalSeconds must be between 15 and 600")
            .When(c => c.SnapshotIntervalSeconds is not null);

        RuleFor(c => c.ViolationWarningThreshold)
            .GreaterThanOrEqualTo(1).WithMessage("ViolationWarningThreshold must be at least 1")
            .When(c => c.ViolationWarningThreshold is not null);
    }
}
