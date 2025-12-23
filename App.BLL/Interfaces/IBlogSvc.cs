using App.BLL.Dtos.BlogDto;
using App.UTIL.Abstractions.DTO.Response;

namespace App.BLL.Interfaces;

public interface IBlogSvc
{
    Task<BaseResponse> GetBlogDetailAsync(int id, CancellationToken ct = default);
    Task<BaseResponse> GetBlogDetailBySlugAsync(string slug, CancellationToken ct = default);
    Task<BaseResponse> GetBlogListAsync(BlogFilter filter, CancellationToken ct = default);
}

