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
    
    public static Error UserAddToRoleFailed(string description) =>
        Error.Failure("Auth.UserAddToRoleFailed", description);

    public static readonly Error TenantNotFound =
        Error.NotFound("Auth.TenantNotFound", "The tenant was not found");

    public static readonly Error AccountLockedOut =
        Error.Forbidden("Auth.AccountLockedOut", "The account is temporarily locked due to too many failed login attempts");

    public static readonly Error EmailNotConfirmed =
        Error.Forbidden("Auth.EmailNotConfirmed", "The email address has not been confirmed yet");

    public static readonly Error InvalidToken =
        Error.Validation("Auth.InvalidToken", "The token is invalid or has expired");

    public static readonly Error UserNotFound =
        Error.NotFound("Auth.UserNotFound", "The user was not found");

    public static readonly Error InvitationAlreadyAccepted =
        Error.Conflict("Auth.InvitationAlreadyAccepted", "This invitation has already been accepted");

    public static readonly Error SelfRegistrationDisabled =
        Error.Forbidden("Auth.SelfRegistrationDisabled", "This tenant does not allow self-registration; contact your tenant administrator");

    public static Error PasswordChangeFailed(string description) =>
        Error.Validation("Auth.PasswordChangeFailed", description);
}
