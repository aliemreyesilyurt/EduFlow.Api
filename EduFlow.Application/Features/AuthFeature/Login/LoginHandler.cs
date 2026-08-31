namespace EduFlow.Application.Features.AuthFeature.Login;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Identity;
using EduFlow.Domain.Abstractions;

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresOn,
    bool RequiresPasswordChange);

public sealed class LoginHandler(IIdentityService identityService) : IHandler<LoginRequest, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> HandleAsync(LoginRequest command, CancellationToken cancellationToken)
    {
        var result = await identityService.LoginAsync(command.Email, command.Password, cancellationToken);

        if (result.IsFailure)
        {
            return Result.Failure<LoginResponse>(result.Error);
        }

        var tokens = result.Value;

        return Result.Success(new LoginResponse(
            tokens.AccessToken, tokens.RefreshToken, tokens.AccessTokenExpiresOn, tokens.RequiresPasswordChange));
    }
}
