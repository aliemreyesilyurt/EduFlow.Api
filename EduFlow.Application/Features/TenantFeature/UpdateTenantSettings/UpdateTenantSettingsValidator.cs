using FluentValidation;

namespace EduFlow.Application.Features.TenantFeature.UpdateTenantSettings;

public sealed class UpdateTenantSettingsValidator : AbstractValidator<UpdateTenantSettingsRequest>
{
    public UpdateTenantSettingsValidator()
    {
        RuleFor(c => c.ProctoringRetentionDays)
            .InclusiveBetween(1, 365).WithMessage("ProctoringRetentionDays must be between 1 and 365");

        RuleFor(c => c.ProctoringConsentText)
            .MaximumLength(4000).WithMessage("ProctoringConsentText must not exceed 4000 characters");
    }
}
