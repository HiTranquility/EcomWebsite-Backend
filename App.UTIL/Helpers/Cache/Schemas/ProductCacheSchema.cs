using System;
using App.UTIL.Helpers.Cache.Builders;

namespace App.UTIL.Helpers.Cache.Schemas;

/// <summary>
/// Cache key builder and TTL configuration for Product domain.
/// </summary>
public static class ProductCacheSchema
{
    public static readonly string ListPrefix = "product:list";
    public static readonly string DetailPrefix = "product:detail";
    public static readonly string FilterPrefix = "product:filters";

    // Use centralized TTLs
    public static readonly TimeSpan ListTtl = CacheTtlDefaults.Short;          // 5 min
    public static readonly TimeSpan DetailTtl = CacheTtlDefaults.Medium;       // 10 min
    public static readonly TimeSpan FilterTtl = CacheTtlDefaults.MediumLong;   // 30 min

    public static string BuildListKey(
        string? keyword,
        string? category,
        string? manufacturer,
        string? productCategory,
        string? productTag,
        string? sort,
        decimal? priceMin,
        decimal? priceMax,
        decimal? minRating,
        bool? isFreeShipping,
        bool? isFlashsale,
        bool? isFeature,
        bool? isSpecial,
        bool? isWeekly,
        bool? isToday,
        bool? isDeal,
        string? sizes,
        int page,
        int pageSize)
    {
        var builder = CacheKeyBuilder.ForPrefix(ListPrefix)
            .AddPart("kw", keyword)
            .AddPart("cat", category)
            .AddPart("manu", manufacturer)
            .AddPart("pcat", productCategory)
            .AddPart("ptag", productTag)
            .AddPart("sort", sort)
            .AddPart("pmin", priceMin)
            .AddPart("pmax", priceMax)
            .AddPart("rating", minRating)
            .AddPart("ship", isFreeShipping)
            .AddPart("flash", isFlashsale)
            .AddPart("feat", isFeature)
            .AddPart("special", isSpecial)
            .AddPart("weekly", isWeekly)
            .AddPart("today", isToday)
            .AddPart("deal", isDeal)
            .AddPart("sizes", sizes)
            .AddPart("page", page)
            .AddPart("size", pageSize);

        return builder.BuildKey();
    }

    public static string BuildDetailKeyById(int id) =>
        CacheKeyBuilder.ForPrefix(DetailPrefix).AddPart("id", id).BuildKey();

    public static string BuildDetailKeyBySlug(string slug) =>
        CacheKeyBuilder.ForPrefix(DetailPrefix).AddPart("slug", slug).BuildKey();

    public static string BuildFilterKey() =>
        CacheKeyBuilder.ForPrefix(FilterPrefix).BuildKey();
}

