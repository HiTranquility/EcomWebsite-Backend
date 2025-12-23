using System.Security.Claims;

namespace App.INFRA.Identity;

public interface ITokenService
{
    string GenerateAccessToken(
        string userId,
        IEnumerable<string> roles,
        IEnumerable<string>? permissions = null,
        IEnumerable<string>? scopes = null,
        string? tenantId = null);

    (string Token, TimeSpan Lifetime, DateTimeOffset ExpiresAt) GenerateRefreshToken(string userId, bool rememberMe = false);
    ClaimsPrincipal? ValidateToken(string token);
}

