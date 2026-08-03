using FluentValidation;

namespace EduFlow.Application.Features.AuthFeature.ResetPassword;

public sealed class ResetPasswordValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordValidator()
    {
        RuleFor(c => c.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(c => c.Token)
            .NotEmpty().WithMessage("Token is required");

        RuleFor(c => c.NewPassword)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long");
    }
}
