using App.BLL.Dtos.BlogDto;
using App.BLL.Dtos.BlogDto.Results;
using App.DAL.BlogModels;
using App.DAL.Repositories;
using App.UTIL.Abstractions.BLL;
using App.UTIL.Abstractions.DTO.Response;
using App.UTIL.Helpers.Cache;
using App.UTIL.Helpers.Jwt;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;

namespace App.BLL.Services;

public class BlogSvc : GenericSvc<BlogRepo, Blog>
{
    private readonly IMapper _mapper;
    private readonly IJwtService _jwtService;
    private readonly ICacheService _cacheService;
    
    public BlogSvc(BlogRepo repo, IMapper mapper, IJwtService jwtService, ICacheService cacheService) : base(repo)
    {
        _mapper = mapper;
        _jwtService = jwtService;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse> GetBlogListsAsync(BlogFilter filter, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();
        var cachePrefix = "blogs:list:";
        var cacheKey = $"{filter.BlogCategory ?? string.Empty}|{filter.BlogTag ?? string.Empty}|{filter.Page}|{filter.PageSize}";

        var (total, items) = await _cacheService.GetOrSetAsync((cacheKey + ":data"), async token =>
        {
            var query = _repo.All.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.BlogCategory))
            {
                query = query.Where(b => b.BlogCategoryJoins.Any(j => j.BlogCategory.Title == filter.BlogCategory));
            }

            if (!string.IsNullOrWhiteSpace(filter.BlogTag))
            {
                query = query.Where(b => b.BlogTagJoins.Any(j => j.BlogTag.Title == filter.BlogTag));
            }

            var totalCount = await query.CountAsync(token);

            var page = Math.Max(1, filter.Page);
            var pageSize = Math.Clamp(filter.PageSize, 1, 100);

            var projected = await query
                .AsSplitQuery()
                .ProjectTo<BlogListRes>(_mapper.ConfigurationProvider)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(token);

            return (totalCount, projected);
        }, ttl: TimeSpan.FromSeconds(60), prefix: cachePrefix, cancellationToken: ct);

        rsp.SetData(new
        {
            page = filter.Page,
            pageSize = filter.PageSize,
            total,
            items
        }, "Get blog lists successfully", 200);
        return rsp;
    }
}