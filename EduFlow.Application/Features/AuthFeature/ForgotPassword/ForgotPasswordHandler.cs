namespace EduFlow.Application.Features.AuthFeature.ForgotPassword;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Identity;
using EduFlow.Application.Abstractions.Notifications;
using EduFlow.Application.Options;
using EduFlow.Domain.Abstractions;
using Microsoft.Extensions.Options;

public sealed record ForgotPasswordRequest(string Email);

public sealed class ForgotPasswordHandler(
    IIdentityService identityService,
    IEmailSender emailSender,
    IOptions<ClientAppOptions> clientAppOptions) : IHandler<ForgotPasswordRequest, Result>
{
    public async Task<Result> HandleAsync(ForgotPasswordRequest command, CancellationToken cancellationToken)
    {
        var tokenResult = await identityService.GeneratePasswordResetTokenAsync(command.Email, cancellationToken);

        if (tokenResult.IsSuccess)
        {
            var email = AuthEmails.PasswordReset(
                clientAppOptions.Value.BaseUrl,
                tokenResult.Value.Email,
                tokenResult.Value.FirstName,
                tokenResult.Value.UserId,
                tokenResult.Value.Token,
                tokenResult.Value.TenantId);

            await emailSender.SendAsync(email, cancellationToken);
        }

        // Always succeed, whether or not the email is registered, to avoid account enumeration.
        return Result.Success();
    }
}
