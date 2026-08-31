using FluentValidation;

namespace EduFlow.Application.Features.AuthFeature.ChangePassword;

public sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordValidator()
    {
        RuleFor(c => c.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");

        RuleFor(c => c.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .NotEqual(c => c.CurrentPassword).WithMessage("New password must be different from the current password");
    }
}
