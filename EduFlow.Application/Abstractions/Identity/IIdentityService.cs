using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Abstractions.Identity;

public interface IIdentityService
{
    Task<Result<Guid>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken);
    Task<Result<AuthTokens>> LoginAsync(string email, string password, CancellationToken cancellationToken);
    Task<Result<AuthTokens>> RefreshAsync(string refreshToken, CancellationToken cancellationToken);
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
}

public sealed record CreateUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? NationalId,
    Guid? TenantId,
    string Role);

public sealed record AuthTokens(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresOn);
