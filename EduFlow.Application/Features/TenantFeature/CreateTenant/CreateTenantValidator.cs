using FluentValidation;

namespace EduFlow.Application.Features.TenantFeature.CreateTenant;

public sealed class CreateTenantValidator : AbstractValidator<CreateTenantRequest>
{
    public CreateTenantValidator()
    {
        RuleFor(c => c.TenantName)
            .NotEmpty().WithMessage("Tenant name is required")
            .MaximumLength(200).WithMessage("Tenant name must not exceed 200 characters");

        RuleFor(c => c.AdminEmail)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address");

        RuleFor(c => c.AdminFirstName)
            .NotEmpty().WithMessage("First name is required");

        RuleFor(c => c.AdminLastName)
            .NotEmpty().WithMessage("Last name is required");
    }
}
