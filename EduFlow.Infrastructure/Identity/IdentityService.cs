using System.Text;
using EduFlow.Application.Abstractions.Identity;
using EduFlow.Application.Features.AuthFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Infrastructure.Authentication;
using EduFlow.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduFlow.Infrastructure.Identity;

internal sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ApplicationDbContext dbContext,
    JwtTokenService jwtTokenService,
    ILogger<IdentityService> logger) : IIdentityService
{
    public async Task<Result<Guid>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            NationalId = request.NationalId,
            TenantId = request.TenantId,
            CreatedOn = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            var description = string.Join("; ", createResult.Errors.Select(e => e.Description));
            return AuthErrors.UserCreationFailed(description);
        }

        await userManager.AddToRoleAsync(user, request.Role);

        return Result.Success(user.Id);
    }

    public async Task<Result<Guid>> CreateInvitedUserAsync(CreateInvitedUserRequest request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            TenantId = request.TenantId,
            CreatedOn = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(user);

        if (!createResult.Succeeded)
        {
            var description = string.Join("; ", createResult.Errors.Select(e => e.Description));
            return AuthErrors.UserCreationFailed(description);
        }

        await userManager.AddToRoleAsync(user, request.Role);

        return Result.Success(user.Id);
    }

    public async Task<Result<AuthTokens>> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return AuthErrors.InvalidCredentials;
        }

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

        if (signInResult.IsLockedOut)
        {
            return AuthErrors.AccountLockedOut;
        }

        if (signInResult.IsNotAllowed)
        {
            return AuthErrors.EmailNotConfirmed;
        }

        if (!signInResult.Succeeded)
        {
            return AuthErrors.InvalidCredentials;
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<Result<AuthTokens>> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var hash = JwtTokenService.HashRefreshToken(refreshToken);
        var stored = await dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hash, cancellationToken);

        if (stored is null)
        {
            return AuthErrors.InvalidRefreshToken;
        }

        if (stored.RevokedOn is not null)
        {
            logger.LogWarning(
                "Refresh token reuse detected for user {UserId}; revoking all active refresh tokens",
                stored.UserId);
            await RevokeAllRefreshTokensAsync(stored.UserId, cancellationToken);
            return AuthErrors.InvalidRefreshToken;
        }

        if (!stored.IsActive)
        {
            return AuthErrors.InvalidRefreshToken;
        }

        var user = await userManager.FindByIdAsync(stored.UserId.ToString());

        if (user is null)
        {
            return AuthErrors.InvalidRefreshToken;
        }

        var (accessToken, expiresOn, newEntity, rawNewToken) = await BuildTokensAsync(user);

        stored.RevokedOn = DateTime.UtcNow;
        stored.ReplacedByTokenId = newEntity.Id;
        dbContext.RefreshTokens.Add(newEntity);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new AuthTokens(accessToken, rawNewToken, expiresOn));
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var hash = JwtTokenService.HashRefreshToken(refreshToken);
        var stored = await dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hash, cancellationToken);

        if (stored is { IsActive: true })
        {
            stored.RevokedOn = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RevokeAllRefreshTokensAsync(Guid userId, CancellationToken cancellationToken)
    {
        await dbContext.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedOn == null)
            .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.RevokedOn, DateTime.UtcNow), cancellationToken);
    }

    public async Task<Result<UserTokenResult>> GenerateEmailConfirmationTokenAsync(string email, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return AuthErrors.UserNotFound;
        }

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        return Result.Success(new UserTokenResult(user.Id, user.Email!, user.FirstName, EncodeToken(token), user.TenantId));
    }

    public async Task<Result> ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return AuthErrors.UserNotFound;
        }

        var result = await userManager.ConfirmEmailAsync(user, DecodeToken(token));

        return result.Succeeded ? Result.Success() : AuthErrors.InvalidToken;
    }

    public async Task<Result<UserTokenResult>> GeneratePasswordResetTokenAsync(string email, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return AuthErrors.UserNotFound;
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        return Result.Success(new UserTokenResult(user.Id, user.Email!, user.FirstName, EncodeToken(token), user.TenantId));
    }

    public async Task<Result> ResetPasswordAsync(Guid userId, string token, string newPassword, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return AuthErrors.UserNotFound;
        }

        var result = await userManager.ResetPasswordAsync(user, DecodeToken(token), newPassword);

        if (!result.Succeeded)
        {
            return AuthErrors.InvalidToken;
        }

        await RevokeAllRefreshTokensAsync(user.Id, cancellationToken);

        return Result.Success();
    }

    public async Task<Result<UserTokenResult>> GenerateInvitationTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return AuthErrors.UserNotFound;
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        return Result.Success(new UserTokenResult(user.Id, user.Email!, user.FirstName, EncodeToken(token), user.TenantId));
    }

    public async Task<Result<AuthTokens>> AcceptInvitationAsync(Guid userId, string token, string password, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return AuthErrors.UserNotFound;
        }

        if (user.EmailConfirmed)
        {
            return AuthErrors.InvitationAlreadyAccepted;
        }

        var result = await userManager.ResetPasswordAsync(user, DecodeToken(token), password);

        if (!result.Succeeded)
        {
            return AuthErrors.InvalidToken;
        }

        user.EmailConfirmed = true;
        await userManager.UpdateAsync(user);

        return await IssueTokensAsync(user, cancellationToken);
    }

    private async Task<Result<AuthTokens>> IssueTokensAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var (accessToken, expiresOn, entity, rawToken) = await BuildTokensAsync(user);

        dbContext.RefreshTokens.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new AuthTokens(accessToken, rawToken, expiresOn));
    }

    private async Task<(string AccessToken, DateTime ExpiresOn, RefreshToken Entity, string RawToken)> BuildTokensAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var (accessToken, expiresOn) = jwtTokenService.GenerateAccessToken(user, roles);
        var rawRefreshToken = JwtTokenService.GenerateRawRefreshToken();

        var entity = new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            TokenHash = JwtTokenService.HashRefreshToken(rawRefreshToken),
            CreatedOn = DateTime.UtcNow,
            ExpiresOn = DateTime.UtcNow.AddDays(jwtTokenService.RefreshTokenDays)
        };

        return (accessToken, expiresOn, entity, rawRefreshToken);
    }

    /// <summary>Identity tokens contain characters unsafe for query strings; encode for email links.</summary>
    private static string EncodeToken(string rawToken) =>
        WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));

    private static string DecodeToken(string encodedToken) =>
        Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(encodedToken));
}
