namespace EduFlow.Application.Features.AuthFeature.RegisterTenant;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Abstractions.Identity;
using EduFlow.Application.Constants;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record RegisterTenantRequest(
    string TenantName,
    string AdminEmail,
    string AdminPassword,
    string AdminFirstName,
    string AdminLastName);

public sealed record RegisterTenantResponse(
    Guid TenantId,
    Guid AdminUserId,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresOn);

public sealed class RegisterTenantHandler(
    IRepository<Tenant> tenantRepository,
    IUnitOfWork unitOfWork,
    IIdentityService identityService) : IHandler<RegisterTenantRequest, Result<RegisterTenantResponse>>
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

        var loginResult = await identityService.LoginAsync(command.AdminEmail, command.AdminPassword, cancellationToken);

        if (loginResult.IsFailure)
        {
            return Result.Failure<RegisterTenantResponse>(loginResult.Error);
        }

        var tokens = loginResult.Value;

        return Result.Success(new RegisterTenantResponse(
            tenant.Id,
            createUserResult.Value,
            tokens.AccessToken,
            tokens.RefreshToken,
            tokens.AccessTokenExpiresOn));
    }

    private static string Slugify(string name) =>
        string.Concat(name.Trim().ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '-')).Trim('-');
}
