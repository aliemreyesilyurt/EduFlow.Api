namespace EduFlow.Application.Features.AuthFeature.ResendConfirmationEmail;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Identity;
using EduFlow.Application.Abstractions.Notifications;
using EduFlow.Application.Options;
using EduFlow.Domain.Abstractions;
using Microsoft.Extensions.Options;

public sealed record ResendConfirmationEmailRequest(string Email);

public sealed class ResendConfirmationEmailHandler(
    IIdentityService identityService,
    IEmailSender emailSender,
    IOptions<ClientAppOptions> clientAppOptions) : IHandler<ResendConfirmationEmailRequest, Result>
{
    public async Task<Result> HandleAsync(ResendConfirmationEmailRequest command, CancellationToken cancellationToken)
    {
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

        // Always succeed, whether or not the email is registered, to avoid account enumeration.
        return Result.Success();
    }
}
