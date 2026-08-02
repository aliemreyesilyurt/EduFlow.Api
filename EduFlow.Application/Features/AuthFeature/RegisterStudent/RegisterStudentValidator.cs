using FluentValidation;

namespace EduFlow.Application.Features.AuthFeature.RegisterStudent;

public sealed class RegisterStudentValidator : AbstractValidator<RegisterStudentRequest>
{
    public RegisterStudentValidator()
    {
        RuleFor(c => c.TenantSlug)
            .NotEmpty().WithMessage("Tenant slug is required");

        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address");

        RuleFor(c => c.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long");

        RuleFor(c => c.FirstName)
            .NotEmpty().WithMessage("First name is required");

        RuleFor(c => c.LastName)
            .NotEmpty().WithMessage("Last name is required");
    }
}
