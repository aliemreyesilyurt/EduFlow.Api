namespace EduFlow.Application.Features.AuthFeature.InviteInstructor;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Identity;
using EduFlow.Application.Constants;
using EduFlow.Domain.Abstractions;

public sealed record InviteInstructorRequest(string Email, string Password, string FirstName, string LastName);

public sealed record InviteInstructorResponse(Guid UserId);

public sealed class InviteInstructorHandler(
    IIdentityService identityService,
    ITenantContext tenantContext) : IHandler<InviteInstructorRequest, Result<InviteInstructorResponse>>
{
    public async Task<Result<InviteInstructorResponse>> HandleAsync(InviteInstructorRequest command, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
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
                TenantId: tenantId,
                Role: Roles.Instructor),
            cancellationToken);

        if (createUserResult.IsFailure)
        {
            return Result.Failure<InviteInstructorResponse>(createUserResult.Error);
        }

        return Result.Success(new InviteInstructorResponse(createUserResult.Value));
    }
}
