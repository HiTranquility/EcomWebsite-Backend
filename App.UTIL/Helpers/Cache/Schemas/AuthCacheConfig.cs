using System;
using App.UTIL.Helpers.Cache.Builders;

namespace App.UTIL.Helpers.Cache.Schemas;

public static class AuthCacheConfig
{
    public const string RefreshPrefix = "auth:refresh";
    public const string ResetPrefix = "auth:reset";
    public const string VerifyPrefix = "auth:verify";

    public static TimeSpan RefreshTokenTtl => TimeSpan.FromDays(7);
    public static TimeSpan ResetTokenTtl => TimeSpan.FromHours(1);
    public static TimeSpan VerifyTokenTtl => TimeSpan.FromDays(1);

    public static string BuildRefreshKey(string hash)
        => CacheKeyBuilder
            .ForPrefix(RefreshPrefix)
            .WithRaw("token", hash, skipIfNullOrEmpty: false)
            .BuildKey();

    public static string BuildResetKey(string hash)
        => CacheKeyBuilder
            .ForPrefix(ResetPrefix)
            .WithRaw("token", hash, skipIfNullOrEmpty: false)
            .BuildKey();

    public static string BuildVerifyKey(string hash)
        => CacheKeyBuilder
            .ForPrefix(VerifyPrefix)
            .WithRaw("token", hash, skipIfNullOrEmpty: false)
            .BuildKey();
}
