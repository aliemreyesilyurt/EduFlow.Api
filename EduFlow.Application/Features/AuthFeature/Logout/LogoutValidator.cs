using FluentValidation;

namespace EduFlow.Application.Features.AuthFeature.Logout;

public sealed class LogoutValidator : AbstractValidator<LogoutRequest>
{
    public LogoutValidator()
    {
        RuleFor(c => c.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");
    }
}
