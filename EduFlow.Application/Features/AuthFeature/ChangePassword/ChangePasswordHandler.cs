namespace EduFlow.Application.Features.AuthFeature.ChangePassword;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Identity;
using EduFlow.Domain.Abstractions;

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public sealed class ChangePasswordHandler(
    IIdentityService identityService,
    ITenantContext tenantContext) : IHandler<ChangePasswordRequest, Result>
{
    public Task<Result> HandleAsync(ChangePasswordRequest command, CancellationToken cancellationToken)
    {
        if (tenantContext.UserId is not { } userId)
        {
            return Task.FromResult<Result>(AuthErrors.UserNotFound);
        }

        return identityService.ChangePasswordAsync(userId, command.CurrentPassword, command.NewPassword, cancellationToken);
    }
}
