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

    public static string? GetUserRole(this ClaimsPrincipal user)
    {
        if (user?.Identity?.IsAuthenticated != true) return null;
        return user.FindFirst(ClaimTypes.Role)?.Value ?? user.FindFirst("role")?.Value;
    }
}
