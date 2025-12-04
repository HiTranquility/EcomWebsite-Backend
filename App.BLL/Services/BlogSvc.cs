using App.BLL.Dtos.BlogDto;
using App.BLL.Dtos.BlogDto.Results;
using App.DAL.BlogModels;
using App.BLL.Dtos.BlogDto.Shares;
using App.DAL.Repositories;
using App.UTIL.Abstractions.BLL;
using App.UTIL.Abstractions.DTO.Response;
using App.UTIL.Helpers.Cache.Schemas;
using App.UTIL.Helpers.Cache;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using LinqKit;


namespace App.BLL.Services;

public class BlogSvc : GenericSvc<BlogRepo, Blog>
{
    private readonly ICacheService _cacheService;

    public BlogSvc(BlogRepo repo, ICacheService cacheService, IMapper mapper) : base(repo, mapper)
    {
        _cacheService = cacheService;
    }

    public async Task<BaseResponse> GetBlogDetailAsync(int id, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();
        var cachePrefix = BlogCacheConfig.DetailPrefix;
        var cacheKey = BlogCacheConfig.BuildDetailKey(id);
        var blog = await _cacheService.GetOrSetAsync(cacheKey, async token =>
        {
            var blogs = await _repo.All
                .AsNoTracking()
                .Include(b => b.BlogVariants)
                .Include(b => b.BlogCategoryJoins)
                    .ThenInclude(j => j.BlogCategory)
                .Include(b => b.BlogTagJoins)
                    .ThenInclude(j => j.BlogTag)
                .Include(b => b.Quote)
                .TagWith("BlogSvc.GetBlogDetailAsync")
                // 🔹 Lấy luôn 3 bài gần nhau (hiện tại, trước, sau)
                .Where(b => b.Id >= id - 1 && b.Id <= id + 1)
                .OrderBy(b => b.Id)
                .ToListAsync(token);
            var detail = blogs.FirstOrDefault(b => b.Id == id);
            if (detail == null)
                return null;

            // ✅ Lấy prev / next trong list hiện tại (nếu có)
            var prev = blogs.Where(b => b.Id < id)
                .OrderByDescending(b => b.Id)
                .Select(b => new NeighborBlogItem
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                })
                .FirstOrDefault();

            var next = blogs.Where(b => b.Id > id)
                .OrderBy(b => b.Id)
                .Select(b => new NeighborBlogItem
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                })
                .FirstOrDefault();

            return _mapper.Map<BlogDetailRes>(detail, options =>
            {
                options.AfterMap((src, dest) =>
                {
                    dest.PrevBlog = prev;
                    dest.NextBlog = next;
                });
            });


        }, ttl: BlogCacheConfig.DetailTtl, prefix: cachePrefix, cancellationToken: ct);
        if (blog == null)
        {
            rsp.SetError("BLOG_NOT_FOUND", "Blog not found", "Blog not found", 404);
            return rsp;
        }

        rsp.SetData(blog, "Get blog detail successfully", 200);
        return rsp;
    }

    public async Task<BaseResponse> GetBlogDetailBySlugAsync(string slug, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();
        if (string.IsNullOrWhiteSpace(slug))
        {
            rsp.SetError("INVALID_SLUG", "Slug is required", "Slug is required", 400);
            return rsp;
        }

        var cachePrefix = BlogCacheConfig.DetailPrefix;
        var cacheKey = BlogCacheConfig.BuildDetailKey(0) + $":slug:{slug}"; // Use slug in cache key
        var blog = await _cacheService.GetOrSetAsync(cacheKey, async token =>
        {
            var detail = await _repo.All
                .AsNoTracking()
                .Include(b => b.BlogVariants)
                .Include(b => b.BlogCategoryJoins)
                    .ThenInclude(j => j.BlogCategory)
                .Include(b => b.BlogTagJoins)
                    .ThenInclude(j => j.BlogTag)
                .Include(b => b.Quote)
                .TagWith("BlogSvc.GetBlogDetailBySlugAsync")
                .Where(b => b.Slug != null && b.Slug == slug && b.DeletedAt == null)
                .FirstOrDefaultAsync(token);

            if (detail == null)
                return null;

            // ✅ Lấy prev / next blog dựa trên CreatedAt
            var prev = await _repo.All
                .AsNoTracking()
                .Where(b => b.CreatedAt < detail.CreatedAt && b.DeletedAt == null)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new NeighborBlogItem
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                })
                .FirstOrDefaultAsync(token);

            var next = await _repo.All
                .AsNoTracking()
                .Where(b => b.CreatedAt > detail.CreatedAt && b.DeletedAt == null)
                .OrderBy(b => b.CreatedAt)
                .Select(b => new NeighborBlogItem
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                })
                .FirstOrDefaultAsync(token);

            return _mapper.Map<BlogDetailRes>(detail, options =>
            {
                options.AfterMap((src, dest) =>
                {
                    dest.PrevBlog = prev;
                    dest.NextBlog = next;
                });
            });

        }, ttl: BlogCacheConfig.DetailTtl, prefix: cachePrefix, cancellationToken: ct);

        if (blog == null)
        {
            rsp.SetError("BLOG_NOT_FOUND", "Blog not found", "Blog not found", 404);
            return rsp;
        }

        rsp.SetData(blog, "Get blog detail successfully", 200);
        return rsp;
    }

    public async Task<BaseResponse> GetBlogListAsync(BlogFilter filter, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();
        filter.Normalize();

        //Build Cache Key
        var cachePrefix = BlogCacheConfig.ListPrefix;
        var cacheKey = BlogCacheConfig.BuildListKey(
            filter.Category,
            filter.Tags,
            filter.Sort,
            filter.Keyword,
            filter.Author,
            filter.Page,
            filter.PageSize);

        //Get Data From Cache

        var (total, items) = await _cacheService.GetOrSetAsync(cacheKey, async token =>
        {
            var query = _repo.All
                .AsNoTracking()
                .AsExpandable()
                .TagWith("BlogSvc.GetBlogListsAsync");

            // CATEGORY
            if (!string.IsNullOrWhiteSpace(filter.Category))
            {
                var category = filter.Category.Trim();
                query = query.Where(b =>
                    b.BlogCategoryJoins.Any(j => j.BlogCategory.Title == category));
            }

            // TAGS
            if (filter.Tags is { Length: > 0 })
            {
                var tagSet = (filter.Tags ?? Array.Empty<string>())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Select(t => t.Trim())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (tagSet.Count > 0)
                {
                    query = query.Where(b =>
                        b.BlogTagJoins.Any(j =>
                            j.BlogTag.Title != null &&
                            tagSet.Contains(j.BlogTag.Title)));
                }
            }

            // KEYWORD
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var kw = filter.Keyword.Trim();
                query = query.Where(b =>
                    EF.Functions.Like(b.Title ?? string.Empty, $"%{kw}%") ||
                    EF.Functions.Like(b.Content ?? string.Empty, $"%{kw}%") ||
                    EF.Functions.Like(b.Author ?? string.Empty, $"%{kw}%"));
            }

            // AUTHOR
            if (!string.IsNullOrWhiteSpace(filter.Author))
            {
                var author = filter.Author.Trim();
                query = query.Where(b =>
                    EF.Functions.Like(b.Author ?? string.Empty, $"%{author}%"));
            }

            var orderedQuery = (filter.Sort?.ToLowerInvariant()) switch
            {
                "oldest" => query.OrderBy(b => b.CreatedAt),
                "newest" => query.OrderByDescending(b => b.CreatedAt),

                // default case
                _ => query.OrderByDescending(b => b.CreatedAt)
            };

            var total = await query.CountAsync(token);

            var projectedItems = await orderedQuery
                .AsSplitQuery()
                .ProjectTo<BlogListRes>(_mapper.ConfigurationProvider)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(token);

            return (total, projectedItems);
        }, ttl: BlogCacheConfig.ListTtl, prefix: cachePrefix, cancellationToken: ct);
        rsp.SetData(new
        {
            Total = (int)total,
            Items = items,
            Page = filter.Page,
            PageSize = filter.PageSize
        });
        return rsp;
    }
}