using System;
using App.UTIL.Helpers.Cache.Builders;

namespace App.UTIL.Helpers.Cache.Schemas;

/// <summary>
/// Cache schema for user-related data.
/// </summary>
public static class UserCacheSchema
{
    public static readonly string ProfilePrefix = "user:profile";
    public static readonly string CommentPrefix = "user:comments";
    public static readonly string WishlistPrefix = "user:wishlist";
    public static readonly string AddressPrefix = "user:addresses";

    // Use centralized TTLs
    public static readonly TimeSpan ProfileTtl = CacheTtlDefaults.Medium;      // 10 min
    public static readonly TimeSpan CommentTtl = CacheTtlDefaults.Short;       // 5 min
    public static readonly TimeSpan WishlistTtl = CacheTtlDefaults.Short;      // 5 min
    public static readonly TimeSpan AddressTtl = CacheTtlDefaults.MediumLong;  // 30 min (rarely changes)

    public static string BuildProfileKey(int userId) =>
        CacheKeyBuilder.ForPrefix(ProfilePrefix).AddPart("user", userId).BuildKey();

    public static string BuildCommentKey(int userId) =>
        CacheKeyBuilder.ForPrefix(CommentPrefix).AddPart("user", userId).BuildKey();

    public static string BuildWishlistKey(int userId) =>
        CacheKeyBuilder.ForPrefix(WishlistPrefix).AddPart("user", userId).BuildKey();

    public static string BuildAddressKey(int userId) =>
        CacheKeyBuilder.ForPrefix(AddressPrefix).AddPart("user", userId).BuildKey();
}

