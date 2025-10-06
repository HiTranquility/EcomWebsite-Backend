using System.Security.Claims;

namespace App.UTIL.Helpers.Jwt;

public interface IJwtService
{
    string GenerateToken(string userId, string role, TimeSpan expiresIn);
    ClaimsPrincipal? ValidateToken(string token);
}