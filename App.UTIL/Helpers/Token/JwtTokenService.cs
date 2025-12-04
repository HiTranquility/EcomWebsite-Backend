using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using App.UTIL.Extensions;

namespace App.UTIL.Helpers.Token;

public class JwtTokenService : ITokenService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly string _secretKey;

    private readonly int _tokenExpiryHours;
    private readonly int _refreshExpiryDays;
    private readonly int _rememberRefreshExpiryDays;

    private readonly JwtSecurityTokenHandler _tokenHandler;

    public JwtTokenService(IConfiguration config)
    {
        var section = config.GetSection("JwtSettings");

        _issuer = section["Issuer"] ?? throw new("Missing JwtSettings:Issuer");
        _audience = section["Audience"] ?? throw new("Missing JwtSettings:Audience");
        _secretKey = section["SecretKey"] ?? throw new("Missing JwtSettings:SecretKey");

        _tokenExpiryHours = int.Parse(section["TokenExpiryInHours"] ?? "24");
        _refreshExpiryDays = int.Parse(section["RefreshTokenExpiryInDays"] ?? "7");
        _rememberRefreshExpiryDays = int.Parse(section["RememberMeRefreshTokenExpiryInDays"] ?? "30");

        _tokenHandler = new JwtSecurityTokenHandler();
    }

    private string CreateToken(List<Claim> claims, TimeSpan expiresIn)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.Add(expiresIn),
            signingCredentials: creds
        );

        return _tokenHandler.WriteToken(token);
    }

    public string GenerateAccessToken(
        string userId,
        IEnumerable<string> roles,
        IEnumerable<string>? permissions = null,
        IEnumerable<string>? scopes = null,
        string? tenantId = null)
    {
        var distinctRoles = (roles ?? Enumerable.Empty<string>())
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => role.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(
                    JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64),
                new Claim("userId", userId),
                new Claim(ClaimTypes.NameIdentifier, userId)
            });

        foreach (var role in distinctRoles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        identity.AddPermissions(permissions ?? Enumerable.Empty<string>());
        identity.AddScopes(scopes ?? Enumerable.Empty<string>());
        if (!string.IsNullOrWhiteSpace(tenantId))
        {
            identity.SetTenant(tenantId);
        }

        return CreateToken(identity.Claims.ToList(), TimeSpan.FromHours(_tokenExpiryHours));
    }

    public (string Token, TimeSpan Lifetime, DateTimeOffset ExpiresAt) GenerateRefreshToken(string userId, bool rememberMe = false)
    {
        var token = TokenHasherExtensions.GenerateToken();
        var days = rememberMe ? _rememberRefreshExpiryDays : _refreshExpiryDays;
        var lifetime = TimeSpan.FromDays(days);
        return (token, lifetime, DateTimeOffset.UtcNow.Add(lifetime));
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // strict expiry
            };

            var principal = _tokenHandler.ValidateToken(token, parameters, out var validated);

            if (validated is not JwtSecurityToken jwt ||
                jwt.Header.Alg != SecurityAlgorithms.HmacSha256)
                return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }
}