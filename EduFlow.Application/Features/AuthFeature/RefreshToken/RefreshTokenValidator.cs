using FluentValidation;

namespace EduFlow.Application.Features.AuthFeature.RefreshToken;

public sealed class RefreshTokenValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenValidator()
    {
        RuleFor(c => c.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");
    }
}
