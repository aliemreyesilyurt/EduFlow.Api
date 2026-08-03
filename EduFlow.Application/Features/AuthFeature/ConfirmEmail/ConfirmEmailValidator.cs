using FluentValidation;

namespace EduFlow.Application.Features.AuthFeature.ConfirmEmail;

public sealed class ConfirmEmailValidator : AbstractValidator<ConfirmEmailRequest>
{
    public ConfirmEmailValidator()
    {
        RuleFor(c => c.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(c => c.Token)
            .NotEmpty().WithMessage("Token is required");
    }
}
