using FluentValidation;

namespace EduFlow.Application.Features.AuthFeature.AcceptInvitation;

public sealed class AcceptInvitationValidator : AbstractValidator<AcceptInvitationRequest>
{
    public AcceptInvitationValidator()
    {
        RuleFor(c => c.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(c => c.Token)
            .NotEmpty().WithMessage("Token is required");

        RuleFor(c => c.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long");
    }
}
