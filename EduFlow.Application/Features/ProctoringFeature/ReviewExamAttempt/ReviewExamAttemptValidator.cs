using FluentValidation;

namespace EduFlow.Application.Features.ProctoringFeature.ReviewExamAttempt;

public sealed class ReviewExamAttemptValidator : AbstractValidator<ReviewExamAttemptRequest>
{
    public ReviewExamAttemptValidator()
    {
        RuleFor(c => c.AttemptId)
            .NotEmpty().WithMessage("AttemptId is required");

        RuleFor(c => c.Note)
            .MaximumLength(2000).WithMessage("Note must not exceed 2000 characters");
    }
}
