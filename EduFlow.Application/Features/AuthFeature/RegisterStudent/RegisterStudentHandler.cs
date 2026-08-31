namespace EduFlow.Application.Features.AuthFeature.RegisterStudent;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Abstractions.Identity;
using EduFlow.Application.Abstractions.Notifications;
using EduFlow.Application.Constants;
using EduFlow.Application.Options;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;
using Microsoft.Extensions.Options;

public sealed record RegisterStudentRequest(
    string TenantSlug,
    string Email,
    string Password,
    string FirstName,
    string LastName);

public sealed record RegisterStudentResponse(Guid UserId);

public sealed class RegisterStudentHandler(
    IRepository<Tenant> tenantRepository,
    IIdentityService identityService,
    IEmailSender emailSender,
    IOptions<ClientAppOptions> clientAppOptions) : IHandler<RegisterStudentRequest, Result<RegisterStudentResponse>>
{
    public async Task<Result<RegisterStudentResponse>> HandleAsync(RegisterStudentRequest command, CancellationToken cancellationToken)
    {
        var tenant = await tenantRepository.FindAsync(t => t.Slug == command.TenantSlug && t.IsActive, cancellationToken);

        if (tenant is null)
        {
            return AuthErrors.TenantNotFound;
        }

        if (!tenant.AllowSelfRegistration)
        {
            return AuthErrors.SelfRegistrationDisabled;
        }

        var createUserResult = await identityService.CreateUserAsync(
            new CreateUserRequest(
                command.Email,
                command.Password,
                command.FirstName,
                command.LastName,
                NationalId: null,
                TenantId: tenant.Id,
                Role: Roles.Student),
            cancellationToken);

        if (createUserResult.IsFailure)
        {
            return Result.Failure<RegisterStudentResponse>(createUserResult.Error);
        }

        var tokenResult = await identityService.GenerateEmailConfirmationTokenAsync(command.Email, cancellationToken);

        if (tokenResult.IsSuccess)
        {
            var email = AuthEmails.EmailVerification(
                clientAppOptions.Value.BaseUrl,
                tokenResult.Value.Email,
                tokenResult.Value.FirstName,
                tokenResult.Value.UserId,
                tokenResult.Value.Token,
                tokenResult.Value.TenantId);

            await emailSender.SendAsync(email, cancellationToken);
        }

        return Result.Success(new RegisterStudentResponse(createUserResult.Value));
    }
}
