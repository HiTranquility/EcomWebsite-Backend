using System;
using App.UTIL.Helpers.Cache.Builders;

namespace App.UTIL.Helpers.Cache.Schemas;

public static class UserCacheConfig
{
    public const string DetailPrefix = "users:detail";
    public const string CommentPrefix = "users:comment";

    public static TimeSpan DetailTtl => TimeSpan.FromMinutes(30);
    public static TimeSpan CommentTtl => TimeSpan.FromMinutes(15);

    public static string BuildDetailKey(int userId) =>
        CacheKeyBuilder
            .ForPrefix(DetailPrefix)
            .With("id", userId)
            .BuildKey();

    public static string BuildCommentKey(int userId) =>
        CacheKeyBuilder
            .ForPrefix(CommentPrefix)
            .With("user", userId)
            .BuildKey();
}

