using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Abstractions.Identity;

public interface IIdentityService
{
    Task<Result<Guid>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken);
    Task<Result<Guid>> CreateInvitedUserAsync(CreateInvitedUserRequest request, CancellationToken cancellationToken);
    Task<Result<AuthTokens>> LoginAsync(string email, string password, CancellationToken cancellationToken);
    Task<Result<AuthTokens>> RefreshAsync(string refreshToken, CancellationToken cancellationToken);
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    Task RevokeAllRefreshTokensAsync(Guid userId, CancellationToken cancellationToken);

    Task<Result<UserTokenResult>> GenerateEmailConfirmationTokenAsync(string email, CancellationToken cancellationToken);
    Task<Result> ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken);

    Task<Result<UserTokenResult>> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken);
    Task<Result> ResetPasswordAsync(Guid userId, string token, string newPassword, CancellationToken cancellationToken);

    Task<Result<UserTokenResult>> GenerateInvitationTokenAsync(Guid userId, CancellationToken cancellationToken);
    Task<Result<AuthTokens>> AcceptInvitationAsync(Guid userId, string token, string password, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<Guid, string>> GetDisplayNamesAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken);
}

public sealed record CreateUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? NationalId,
    Guid? TenantId,
    string Role);

public sealed record CreateInvitedUserRequest(
    string Email,
    string FirstName,
    string LastName,
    Guid TenantId,
    string Role);

public sealed record AuthTokens(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresOn);

public sealed record UserTokenResult(Guid UserId, string Email, string FirstName, string Token, Guid? TenantId);
