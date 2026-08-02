using EduFlow.Domain.Abstractions.Errors;

namespace EduFlow.Application.Features.AuthFeature;

public static class AuthErrors
{
    public static readonly Error InvalidCredentials =
        Error.Unauthorized("Auth.InvalidCredentials", "Email or password is incorrect");

    public static readonly Error InvalidRefreshToken =
        Error.Unauthorized("Auth.InvalidRefreshToken", "The refresh token is invalid, expired or revoked");

    public static Error UserCreationFailed(string description) =>
        Error.Failure("Auth.UserCreationFailed", description);

    public static readonly Error TenantNotFound =
        Error.NotFound("Auth.TenantNotFound", "The tenant was not found");
}
