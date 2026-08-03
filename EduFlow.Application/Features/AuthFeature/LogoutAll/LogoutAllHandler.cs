namespace EduFlow.Application.Features.AuthFeature.LogoutAll;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Identity;
using EduFlow.Domain.Abstractions;

public sealed record LogoutAllRequest;

public sealed class LogoutAllHandler(
    IIdentityService identityService,
    ITenantContext tenantContext) : IHandler<LogoutAllRequest, Result>
{
    public async Task<Result> HandleAsync(LogoutAllRequest command, CancellationToken cancellationToken)
    {
        if (tenantContext.UserId is not { } userId)
        {
            return AuthErrors.UserNotFound;
        }

        await identityService.RevokeAllRefreshTokensAsync(userId, cancellationToken);

        return Result.Success();
    }
}
