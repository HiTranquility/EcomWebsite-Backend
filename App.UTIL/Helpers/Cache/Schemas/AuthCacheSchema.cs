using App.UTIL.Helpers.Cache.Builders;

namespace App.UTIL.Helpers.Cache.Schemas;

/// <summary>
/// Cache key builder and TTLs for Auth/Token domain.
/// </summary>
public static class AuthCacheSchema
{
    public static readonly string RefreshTokenPrefix = "auth:refresh_token";
    public static readonly string UserSessionPrefix = "auth:session";
    public static readonly string UserPermissionsPrefix = "auth:permissions";
    public static readonly string FailedLoginPrefix = "auth:failed_login";

    // Use centralized TTLs
    public static readonly TimeSpan RefreshTokenTtl = CacheTtlDefaults.RefreshToken;  // 7 days
    public static readonly TimeSpan SessionTtl = CacheTtlDefaults.Session;             // 30 min
    public static readonly TimeSpan PermissionsTtl = CacheTtlDefaults.MediumLong;      // 30 min
    public static readonly TimeSpan FailedLoginTtl = CacheTtlDefaults.Medium;          // 10 min (rate limiting)

    /// <summary>
    /// Build key for refresh token cache (store token hash -> user mapping)
    /// </summary>
    public static string BuildRefreshTokenKey(string tokenHash) =>
        CacheKeyBuilder.ForPrefix(RefreshTokenPrefix).AddPart("hash", tokenHash).BuildKey();

    /// <summary>
    /// Build key for user session data
    /// </summary>
    public static string BuildSessionKey(int userId) =>
        CacheKeyBuilder.ForPrefix(UserSessionPrefix).AddPart("user", userId).BuildKey();

    /// <summary>
    /// Build key for user permissions cache
    /// </summary>
    public static string BuildPermissionsKey(int userId) =>
        CacheKeyBuilder.ForPrefix(UserPermissionsPrefix).AddPart("user", userId).BuildKey();

    /// <summary>
    /// Build key for tracking failed login attempts (rate limiting)
    /// </summary>
    public static string BuildFailedLoginKey(string identifier) =>
        CacheKeyBuilder.ForPrefix(FailedLoginPrefix).AddPart("id", identifier).BuildKey();
}
