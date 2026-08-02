using FluentValidation;

namespace EduFlow.Application.Features.AuthFeature.Login;

public sealed class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address");

        RuleFor(c => c.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}
