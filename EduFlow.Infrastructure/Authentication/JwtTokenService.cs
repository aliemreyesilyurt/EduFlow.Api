using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EduFlow.Infrastructure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EduFlow.Infrastructure.Authentication;

/// <summary>
/// Internal helper for issuing access/refresh tokens. Not exposed outside Infrastructure -
/// consumers depend on IIdentityService instead.
/// </summary>
internal sealed class JwtTokenService(IOptions<JwtOptions> options)
{
    private readonly JwtOptions _options = options.Value;

    public (string Token, DateTime ExpiresOn) GenerateAccessToken(ApplicationUser user, IEnumerable<string> roles)
    {
        var expiresOn = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);

        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            ..roles.Select(role => new Claim(ClaimTypes.Role, role))
        ];

        if (user.TenantId is { } tenantId)
        {
            claims.Add(new Claim("tenant_id", tenantId.ToString()));
        }

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresOn,
            signingCredentials: signingCredentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresOn);
    }

    public static string GenerateRawRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    public static string HashRefreshToken(string rawToken) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

    public int RefreshTokenDays => _options.RefreshTokenDays;
}
