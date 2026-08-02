namespace EduFlow.Application.Features.AuthFeature.RegisterStudent;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Abstractions.Identity;
using EduFlow.Application.Constants;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record RegisterStudentRequest(
    string TenantSlug,
    string Email,
    string Password,
    string FirstName,
    string LastName);

public sealed record RegisterStudentResponse(
    Guid UserId,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresOn);

public sealed class RegisterStudentHandler(
    IRepository<Tenant> tenantRepository,
    IIdentityService identityService) : IHandler<RegisterStudentRequest, Result<RegisterStudentResponse>>
{
    public async Task<Result<RegisterStudentResponse>> HandleAsync(RegisterStudentRequest command, CancellationToken cancellationToken)
    {
        var tenant = await tenantRepository.FindAsync(t => t.Slug == command.TenantSlug && t.IsActive, cancellationToken);

        if (tenant is null)
        {
            return AuthErrors.TenantNotFound;
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

        var loginResult = await identityService.LoginAsync(command.Email, command.Password, cancellationToken);

        if (loginResult.IsFailure)
        {
            return Result.Failure<RegisterStudentResponse>(loginResult.Error);
        }

        var tokens = loginResult.Value;

        return Result.Success(new RegisterStudentResponse(
            createUserResult.Value,
            tokens.AccessToken,
            tokens.RefreshToken,
            tokens.AccessTokenExpiresOn));
    }
}
