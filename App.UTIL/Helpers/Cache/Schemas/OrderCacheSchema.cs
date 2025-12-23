using App.UTIL.Helpers.Cache.Builders;

namespace App.UTIL.Helpers.Cache.Schemas;

/// <summary>
/// Cache key builder and TTLs for Order domain.
/// </summary>
public static class OrderCacheSchema
{
    public static readonly string ListPrefix = "order:list";
    public static readonly string DetailPrefix = "order:detail";
    public static readonly string UserOrdersPrefix = "order:user";
    public static readonly string StatusPrefix = "order:status";

    // Use centralized TTLs
    public static readonly TimeSpan ListTtl = CacheTtlDefaults.Short;        // 5 min - orders change frequently
    public static readonly TimeSpan DetailTtl = CacheTtlDefaults.Medium;     // 10 min
    public static readonly TimeSpan UserOrdersTtl = CacheTtlDefaults.Short;  // 5 min

    public static string BuildDetailKey(int orderId) =>
        CacheKeyBuilder.ForPrefix(DetailPrefix).AddPart("id", orderId).BuildKey();

    public static string BuildDetailKeyByCode(string orderCode) =>
        CacheKeyBuilder.ForPrefix(DetailPrefix).AddPart("code", orderCode).BuildKey();

    public static string BuildUserOrdersKey(int userId, int? page = null, int? pageSize = null) =>
        CacheKeyBuilder.ForPrefix(UserOrdersPrefix)
            .AddPart("user", userId)
            .AddPart("page", page)
            .AddPart("size", pageSize)
            .BuildKey();

    public static string BuildStatusKey(int orderId) =>
        CacheKeyBuilder.ForPrefix(StatusPrefix).AddPart("id", orderId).BuildKey();
}
