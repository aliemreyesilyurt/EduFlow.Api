using EduFlow.Application.Abstractions.Identity;
using EduFlow.Application.Features.AuthFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Infrastructure.Authentication;
using EduFlow.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduFlow.Infrastructure.Identity;

internal sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext dbContext,
    JwtTokenService jwtTokenService) : IIdentityService
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

    public async Task<Result<AuthTokens>> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null || !await userManager.CheckPasswordAsync(user, password))
        {
            return AuthErrors.InvalidCredentials;
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<Result<AuthTokens>> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var hash = JwtTokenService.HashRefreshToken(refreshToken);
        var stored = await dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hash, cancellationToken);

        if (stored is null || !stored.IsActive)
        {
            return AuthErrors.InvalidRefreshToken;
        }

        var user = await userManager.FindByIdAsync(stored.UserId.ToString());

        if (user is null)
        {
            return AuthErrors.InvalidRefreshToken;
        }

        stored.RevokedOn = DateTime.UtcNow;

        return await IssueTokensAsync(user, cancellationToken);
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

    private async Task<Result<AuthTokens>> IssueTokensAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var roles = await userManager.GetRolesAsync(user);
        var (accessToken, expiresOn) = jwtTokenService.GenerateAccessToken(user, roles);
        var rawRefreshToken = JwtTokenService.GenerateRawRefreshToken();

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            UserId = user.Id,
            TokenHash = JwtTokenService.HashRefreshToken(rawRefreshToken),
            CreatedOn = DateTime.UtcNow,
            ExpiresOn = DateTime.UtcNow.AddDays(jwtTokenService.RefreshTokenDays)
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new AuthTokens(accessToken, rawRefreshToken, expiresOn));
    }
}
