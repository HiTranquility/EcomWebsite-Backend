using System;
using System.Collections.Generic;
using System.Linq;
using App.UTIL.Helpers.Cache.Builders;

namespace App.UTIL.Helpers.Cache.Schemas;

public static class BlogCacheConfig
{
    public const string DetailPrefix = "blogs:detail";
    public const string ListPrefix = "blogs:list";
    public const string CommentsPrefix = "blogs:comments";

    public static TimeSpan DetailTtl => TimeSpan.FromSeconds(60);
    public static TimeSpan ListTtl => TimeSpan.FromSeconds(60);
    public static TimeSpan CommentsTtl => TimeSpan.FromSeconds(60);

    public static string BuildDetailKey(int blogId)
        => CacheKeyBuilder
            .ForPrefix(DetailPrefix)
            .With("id", blogId)
            .BuildKey();

    public static string BuildListKey(
        string? category,
        IEnumerable<string>? tags,
        string? sort,
        string? keyword,
        string? author,
        int page,
        int pageSize)
    {
        var normalizedTags = tags?
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim())
            .ToArray();

        return CacheKeyBuilder
            .ForPrefix(ListPrefix)
            .With("cat", category)
            .With("sort", sort)
            .With("p", page)
            .With("ps", pageSize)
            .WithHashed("filters",
                ("tags", normalizedTags),
                ("keyword", keyword),
                ("author", author))
            .BuildKey();
    }

    public static string BuildCommentKey(int blogId)
        => CacheKeyBuilder
            .ForPrefix(CommentsPrefix)
            .With("blog", blogId)
            .BuildKey();
}