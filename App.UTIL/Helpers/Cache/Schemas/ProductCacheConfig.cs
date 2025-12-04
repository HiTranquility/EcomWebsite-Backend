using System;
using App.UTIL.Helpers.Cache.Builders;

namespace App.UTIL.Helpers.Cache.Schemas;

public static class ProductCacheConfig
{
    public const string DetailPrefix = "products:detail";
    public const string ListPrefix = "products:list";
    public const string ReviewListPrefix = "products:reviews";
    public const string FilterPrefix = "products:filters";

    public static TimeSpan DetailTtl => TimeSpan.FromSeconds(60);
    public static TimeSpan ListTtl => TimeSpan.FromSeconds(60);
    public static TimeSpan ReviewListTtl => TimeSpan.FromSeconds(60);
    public static TimeSpan FilterTtl => TimeSpan.FromSeconds(60);

    public static string BuildDetailKeyById(int productId)
        => CacheKeyBuilder
            .ForPrefix(DetailPrefix)
            .With("id", productId)
            .BuildKey();

    public static string BuildDetailKeyBySlug(string slug)
        => CacheKeyBuilder
            .ForPrefix(DetailPrefix)
            .With("slug", slug)
            .BuildKey();

    public static string BuildListKey(
        string? keyword,
        string? category,
        string? manufacturer,
        string? productCategory,
        string? productTag,
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
        => CacheKeyBuilder
            .ForPrefix(ListPrefix)
            .With("p", page)
            .With("ps", pageSize)
            .WithHashed("filters",
                ("keyword", keyword),
                ("category", category),
                ("manufacturer", manufacturer),
                ("productCategory", productCategory),
                ("productTag", productTag),
                ("priceMin", priceMin),
                ("priceMax", priceMax),
                ("minRating", minRating),
                ("isFreeShipping", isFreeShipping),
                ("isFlashsale", isFlashsale),
                ("isFeature", isFeature),
                ("isSpecial", isSpecial),
                ("isWeekly", isWeekly),
                ("isToday", isToday),
                ("isDeal", isDeal),
                ("sizes", sizes))
            .BuildKey();

    public static string BuildReviewListKey(int productId)
        => CacheKeyBuilder
            .ForPrefix(ReviewListPrefix)
            .With("product", productId)
            .BuildKey();

    public static string BuildFilterKey(string scope = "default")
        => CacheKeyBuilder
            .ForPrefix(FilterPrefix)
            .With("name", "filters")
            .With("scope", scope)
            .BuildKey();
}

