using System.Security.Claims;

namespace App.UTIL.Extensions;

public static class ClaimsIdentityExtensions
{
    public static void AddPermissions(this ClaimsIdentity identity, IEnumerable<string> permissions)
    {
        if (identity == null || permissions == null) return;
        foreach (var permission in permissions)
        {
            if (!string.IsNullOrWhiteSpace(permission))
                identity.AddClaim(new Claim("Permission", permission));
        }
    }

    public static void AddScopes(this ClaimsIdentity identity, IEnumerable<string> scopes)
    {
        if (identity == null || scopes == null) return;
        foreach (var scope in scopes)
        {
            if (!string.IsNullOrWhiteSpace(scope))
                identity.AddClaim(new Claim("scope", scope));
        }
    }

    public static void SetTenant(this ClaimsIdentity identity, string tenantId)
    {
        if (identity == null || string.IsNullOrWhiteSpace(tenantId)) return;
        identity.AddClaim(new Claim("tenant", tenantId));
    }
}