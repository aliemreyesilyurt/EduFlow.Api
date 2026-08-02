namespace EduFlow.Application.Features.AuthFeature.RefreshToken;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Identity;
using EduFlow.Domain.Abstractions;

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record RefreshTokenResponse(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresOn);

public sealed class RefreshTokenHandler(IIdentityService identityService)
    : IHandler<RefreshTokenRequest, Result<RefreshTokenResponse>>
{
    public async Task<Result<RefreshTokenResponse>> HandleAsync(RefreshTokenRequest command, CancellationToken cancellationToken)
    {
        var result = await identityService.RefreshAsync(command.RefreshToken, cancellationToken);

        if (result.IsFailure)
        {
            return Result.Failure<RefreshTokenResponse>(result.Error);
        }

        var tokens = result.Value;

        return Result.Success(new RefreshTokenResponse(tokens.AccessToken, tokens.RefreshToken, tokens.AccessTokenExpiresOn));
    }
}
