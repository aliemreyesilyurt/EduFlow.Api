namespace EduFlow.Application.Features.AuthFeature.InviteInstructor;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Identity;
using EduFlow.Application.Abstractions.Notifications;
using EduFlow.Application.Constants;
using EduFlow.Application.Options;
using EduFlow.Domain.Abstractions;
using Microsoft.Extensions.Options;

public sealed record InviteInstructorRequest(string Email, string FirstName, string LastName);

public sealed record InviteInstructorResponse(Guid UserId);

public sealed class InviteInstructorHandler(
    IIdentityService identityService,
    ITenantContext tenantContext,
    IEmailSender emailSender,
    IOptions<ClientAppOptions> clientAppOptions) : IHandler<InviteInstructorRequest, Result<InviteInstructorResponse>>
{
    public async Task<Result<InviteInstructorResponse>> HandleAsync(InviteInstructorRequest command, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
        {
            return AuthErrors.TenantNotFound;
        }

        var createUserResult = await identityService.CreateInvitedUserAsync(
            new CreateInvitedUserRequest(
                command.Email,
                command.FirstName,
                command.LastName,
                tenantId,
                Roles.Instructor),
            cancellationToken);

        if (createUserResult.IsFailure)
        {
            return Result.Failure<InviteInstructorResponse>(createUserResult.Error);
        }

        var tokenResult = await identityService.GenerateInvitationTokenAsync(createUserResult.Value, cancellationToken);

        if (tokenResult.IsSuccess)
        {
            var email = AuthEmails.InstructorInvitation(
                clientAppOptions.Value.BaseUrl,
                tokenResult.Value.Email,
                tokenResult.Value.FirstName,
                tokenResult.Value.UserId,
                tokenResult.Value.Token,
                tokenResult.Value.TenantId);

            await emailSender.SendAsync(email, cancellationToken);
        }

        return Result.Success(new InviteInstructorResponse(createUserResult.Value));
    }
}
