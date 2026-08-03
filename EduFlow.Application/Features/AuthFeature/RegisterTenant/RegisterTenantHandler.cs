namespace EduFlow.Application.Features.AuthFeature.RegisterTenant;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Abstractions.Identity;
using EduFlow.Application.Abstractions.Notifications;
using EduFlow.Application.Constants;
using EduFlow.Application.Options;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;
using Microsoft.Extensions.Options;

public sealed record RegisterTenantRequest(
    string TenantName,
    string AdminEmail,
    string AdminPassword,
    string AdminFirstName,
    string AdminLastName);

public sealed record RegisterTenantResponse(Guid TenantId, Guid AdminUserId);

public sealed class RegisterTenantHandler(
    IRepository<Tenant> tenantRepository,
    IUnitOfWork unitOfWork,
    IIdentityService identityService,
    IEmailSender emailSender,
    IOptions<ClientAppOptions> clientAppOptions) : IHandler<RegisterTenantRequest, Result<RegisterTenantResponse>>
{
    public async Task<Result<RegisterTenantResponse>> HandleAsync(RegisterTenantRequest command, CancellationToken cancellationToken)
    {
        var tenant = new Tenant
        {
            Id = Guid.CreateVersion7(),
            Name = command.TenantName,
            Slug = Slugify(command.TenantName)
        };

        await tenantRepository.AddAsync(tenant, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        var createUserResult = await identityService.CreateUserAsync(
            new CreateUserRequest(
                command.AdminEmail,
                command.AdminPassword,
                command.AdminFirstName,
                command.AdminLastName,
                NationalId: null,
                TenantId: tenant.Id,
                Role: Roles.TenantAdmin),
            cancellationToken);

        if (createUserResult.IsFailure)
        {
            return Result.Failure<RegisterTenantResponse>(createUserResult.Error);
        }

        var tokenResult = await identityService.GenerateEmailConfirmationTokenAsync(command.AdminEmail, cancellationToken);

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

        return Result.Success(new RegisterTenantResponse(tenant.Id, createUserResult.Value));
    }

    private static string Slugify(string name) =>
        string.Concat(name.Trim().ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '-')).Trim('-');
}
