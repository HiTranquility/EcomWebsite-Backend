using System;
using App.UTIL.Helpers.Cache.Builders;

namespace App.UTIL.Helpers.Cache.Schemas;

/// <summary>
/// Cache key builder and TTLs for Blog domain.
/// </summary>
public static class BlogCacheSchema
{
    public static readonly string DetailPrefix = "blog:detail";
    public static readonly string ListPrefix = "blog:list";

    // Use centralized TTLs
    public static readonly TimeSpan DetailTtl = CacheTtlDefaults.Medium;  // 10 min
    public static readonly TimeSpan ListTtl = CacheTtlDefaults.Short;     // 5 min

    public static string BuildDetailKey(int id) =>
        CacheKeyBuilder.ForPrefix(DetailPrefix).AddPart("id", id).BuildKey();

    public static string BuildListKey(
        string? category,
        string[]? tags,
        string? sort,
        string? keyword,
        string? author,
        int? page,
        int? pageSize)
    {
        var tagValue = tags != null && tags.Length > 0
            ? string.Join(",", tags)
            : null;

        var builder = CacheKeyBuilder.ForPrefix(ListPrefix)
            .AddPart("cat", category)
            .AddPart("tags", tagValue)
            .AddPart("sort", sort)
            .AddPart("kw", keyword)
            .AddPart("author", author)
            .AddPart("page", page)
            .AddPart("size", pageSize);

        return builder.BuildKey();
    }
}

