namespace EduFlow.Application.Features.AuthFeature.ResetPassword;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Identity;
using EduFlow.Domain.Abstractions;

public sealed record ResetPasswordRequest(Guid UserId, string Token, string NewPassword);

public sealed class ResetPasswordHandler(IIdentityService identityService) : IHandler<ResetPasswordRequest, Result>
{
    public Task<Result> HandleAsync(ResetPasswordRequest command, CancellationToken cancellationToken) =>
        identityService.ResetPasswordAsync(command.UserId, command.Token, command.NewPassword, cancellationToken);
}
