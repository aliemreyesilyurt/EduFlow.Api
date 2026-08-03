using FluentValidation;

namespace EduFlow.Application.Features.AuthFeature.InviteInstructor;

public sealed class InviteInstructorValidator : AbstractValidator<InviteInstructorRequest>
{
    public InviteInstructorValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address");

        RuleFor(c => c.FirstName)
            .NotEmpty().WithMessage("First name is required");

        RuleFor(c => c.LastName)
            .NotEmpty().WithMessage("Last name is required");
    }
}
