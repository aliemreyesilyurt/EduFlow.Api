using FluentValidation;

namespace EduFlow.Application.Features.SystemSettingsFeature.UpsertSystemSetting;

public sealed class UpsertSystemSettingValidator : AbstractValidator<UpsertSystemSettingRequest>
{
    public UpsertSystemSettingValidator()
    {
        RuleFor(c => c.Key)
            .NotEmpty().WithMessage("Key is required")
            .MaximumLength(200).WithMessage("Key must not exceed 200 characters");
    }
}
