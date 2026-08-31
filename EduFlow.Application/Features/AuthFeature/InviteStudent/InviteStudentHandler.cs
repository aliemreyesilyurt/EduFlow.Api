namespace EduFlow.Application.Features.AuthFeature.InviteStudent;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Identity;
using EduFlow.Application.Abstractions.Notifications;
using EduFlow.Application.Constants;
using EduFlow.Application.Options;
using EduFlow.Domain.Abstractions;
using Microsoft.Extensions.Options;

public sealed record InviteStudentRequest(string Email, string FirstName, string LastName);

public sealed record InviteStudentResponse(Guid UserId);

public sealed class InviteStudentHandler(
    IIdentityService identityService,
    ITenantContext tenantContext,
    IEmailSender emailSender,
    IOptions<ClientAppOptions> clientAppOptions) : IHandler<InviteStudentRequest, Result<InviteStudentResponse>>
{
    public async Task<Result<InviteStudentResponse>> HandleAsync(InviteStudentRequest command, CancellationToken cancellationToken)
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
                Roles.Student),
            cancellationToken);

        if (createUserResult.IsFailure)
        {
            return Result.Failure<InviteStudentResponse>(createUserResult.Error);
        }

        var tokenResult = await identityService.GenerateInvitationTokenAsync(createUserResult.Value, cancellationToken);

        if (tokenResult.IsSuccess)
        {
            var email = AuthEmails.StudentInvitation(
                clientAppOptions.Value.BaseUrl,
                tokenResult.Value.Email,
                tokenResult.Value.FirstName,
                tokenResult.Value.UserId,
                tokenResult.Value.Token,
                tokenResult.Value.TenantId);

            await emailSender.SendAsync(email, cancellationToken);
        }

        return Result.Success(new InviteStudentResponse(createUserResult.Value));
    }
}
