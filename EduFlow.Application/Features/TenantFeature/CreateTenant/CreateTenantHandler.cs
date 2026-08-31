namespace EduFlow.Application.Features.TenantFeature.CreateTenant;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Abstractions.Identity;
using EduFlow.Application.Constants;
using EduFlow.Application.Features.AuthFeature;
using EduFlow.Application.Options;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;
using Microsoft.Extensions.Options;

public sealed record CreateTenantRequest(
    string TenantName,
    string AdminEmail,
    string AdminFirstName,
    string AdminLastName);

public sealed record CreateTenantResponse(
    Guid TenantId,
    Guid AdminUserId,
    string AdminEmail,
    string TemporaryPassword);

public sealed class CreateTenantHandler(
    IRepository<Tenant> tenantRepository,
    IUnitOfWork unitOfWork,
    IIdentityService identityService,
    IOptions<TenantProvisioningOptions> tenantProvisioningOptions) : IHandler<CreateTenantRequest, Result<CreateTenantResponse>>
{
    public async Task<Result<CreateTenantResponse>> HandleAsync(CreateTenantRequest command, CancellationToken cancellationToken)
    {
        var tenant = new Tenant
        {
            Id = Guid.CreateVersion7(),
            Name = command.TenantName,
            Slug = Slugify(command.TenantName)
        };

        await tenantRepository.AddAsync(tenant, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        var defaultPassword = tenantProvisioningOptions.Value.DefaultAdminPassword;

        var createUserResult = await identityService.CreateUserAsync(
            new CreateUserRequest(
                command.AdminEmail,
                defaultPassword,
                command.AdminFirstName,
                command.AdminLastName,
                NationalId: null,
                TenantId: tenant.Id,
                Role: Roles.TenantAdmin,
                EmailConfirmed: true,
                MustChangePassword: true),
            cancellationToken);

        if (createUserResult.IsFailure)
        {
            return Result.Failure<CreateTenantResponse>(createUserResult.Error);
        }

        return Result.Success(new CreateTenantResponse(
            tenant.Id, createUserResult.Value, command.AdminEmail, defaultPassword));
    }

    private static string Slugify(string name) =>
        string.Concat(name.Trim().ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '-')).Trim('-');
}
