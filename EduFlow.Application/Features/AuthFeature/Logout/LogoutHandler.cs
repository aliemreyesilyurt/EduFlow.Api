namespace EduFlow.Application.Features.AuthFeature.Logout;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Identity;
using EduFlow.Domain.Abstractions;

public sealed record LogoutRequest(string RefreshToken);

public sealed class LogoutHandler(IIdentityService identityService) : IHandler<LogoutRequest, Result>
{
    public async Task<Result> HandleAsync(LogoutRequest command, CancellationToken cancellationToken)
    {
        await identityService.RevokeRefreshTokenAsync(command.RefreshToken, cancellationToken);

        return Result.Success();
    }
}
