using FluentValidation;

namespace EduFlow.Application.Features.ProctoringFeature.GiveProctoringConsent;

public sealed class GiveProctoringConsentValidator : AbstractValidator<GiveProctoringConsentRequest>
{
    public GiveProctoringConsentValidator()
    {
        RuleFor(c => c.AttemptId)
            .NotEmpty().WithMessage("AttemptId is required");
    }
}
