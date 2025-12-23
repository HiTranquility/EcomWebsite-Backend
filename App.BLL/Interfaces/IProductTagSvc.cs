using App.BLL.Dtos.ProductDto.Requests;
using App.UTIL.Abstractions.DTO.Response;

namespace App.BLL.Interfaces;

public interface IProductTagSvc
{
    Task<BaseResponse> GetProductTagsAsync(int? userId, int productId, CancellationToken ct = default);
    Task<BaseResponse> AddProductTagAsync(int userId, int productId, CreateProductTagReq request, CancellationToken ct = default);
    Task<BaseResponse> DeleteProductTagAsync(int userId, int tagId, CancellationToken ct = default);
}

