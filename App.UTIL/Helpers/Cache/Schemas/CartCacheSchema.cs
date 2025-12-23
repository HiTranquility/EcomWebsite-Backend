using App.UTIL.Helpers.Cache.Builders;

namespace App.UTIL.Helpers.Cache.Schemas;

/// <summary>
/// Cache key builder and TTLs for Cart domain.
/// </summary>
public static class CartCacheSchema
{
    public static readonly string CartPrefix = "cart";
    public static readonly string CartItemsPrefix = "cart:items";
    public static readonly string CartTotalPrefix = "cart:total";

    // Use centralized TTLs - cart data is user-specific and volatile
    public static readonly TimeSpan CartTtl = CacheTtlDefaults.Short;          // 5 min
    public static readonly TimeSpan CartItemsTtl = CacheTtlDefaults.Short;     // 5 min
    public static readonly TimeSpan CartTotalTtl = CacheTtlDefaults.ExtraShort; // 1 min - totals change frequently

    public static string BuildCartKey(int userId) =>
        CacheKeyBuilder.ForPrefix(CartPrefix).AddPart("user", userId).BuildKey();

    public static string BuildCartItemsKey(int userId) =>
        CacheKeyBuilder.ForPrefix(CartItemsPrefix).AddPart("user", userId).BuildKey();

    public static string BuildCartTotalKey(int userId) =>
        CacheKeyBuilder.ForPrefix(CartTotalPrefix).AddPart("user", userId).BuildKey();
}
