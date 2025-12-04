using System.Security.Claims;

namespace App.UTIL.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int? GetUserId(this ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true) return null;

        var id = user.FindFirst("userId")?.Value
                 ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? user.FindFirst("sub")?.Value;

        return int.TryParse(id, out var userId) ? userId : (int?)null;
    }

    public static string? GetUsername(this ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true) return null;
        return user.FindFirst(ClaimTypes.Name)?.Value
            ?? user.FindFirst("username")?.Value
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public static string? GetEmail(this ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true) return null;
        return user.FindFirst(ClaimTypes.Email)?.Value
            ?? user.FindFirst("email")?.Value;
    }

    public static string? GetUserRole(this ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true) return null;
        return user.FindFirst(ClaimTypes.Role)?.Value
            ?? user.FindFirst("role")?.Value;
    }

    public static string? GetFullName(this ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true) return null;
        return user.FindFirst("name")?.Value
            ?? user.FindFirst(ClaimTypes.GivenName)?.Value
            ?? user.FindFirst(ClaimTypes.Name)?.Value;
    }

    public static string? GetJti(this ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true) return null;
        return user.FindFirst("jti")?.Value;
    }
    public static IEnumerable<string> GetPermissions(this ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true) return Enumerable.Empty<string>();
        return user.FindAll("Permission")
            .Concat(user.FindAll("permission"))
            .Select(c => c.Value);
    }

    public static bool HasPermission(this ClaimsPrincipal user, string permission)
    {
        if (string.IsNullOrWhiteSpace(permission)) return false;
        return user.GetPermissions().Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    public static IEnumerable<string> GetScopes(this ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true) return Enumerable.Empty<string>();
        return user.FindAll("scope").Select(c => c.Value);
    }

    public static string? GetTenant(this ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true) return null;
        return user.FindFirst("tenant")?.Value;
    }
}
