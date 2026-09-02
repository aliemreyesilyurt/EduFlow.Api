using FluentValidation;

namespace EduFlow.Application.Features.ProctoringFeature.LogProctoringEvents;

public sealed class LogProctoringEventsValidator : AbstractValidator<LogProctoringEventsRequest>
{
    public LogProctoringEventsValidator()
    {
        RuleFor(c => c.AttemptId)
            .NotEmpty().WithMessage("AttemptId is required");

        RuleFor(c => c.Events)
            .NotNull().WithMessage("Events is required");

        RuleForEach(c => c.Events)
            .ChildRules(events =>
            {
                events.RuleFor(e => e.Details)
                    .MaximumLength(500).WithMessage("Details must not exceed 500 characters");
            })
            .When(c => c.Events is not null);
    }
}
