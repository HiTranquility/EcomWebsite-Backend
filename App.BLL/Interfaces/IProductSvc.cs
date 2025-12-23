using App.BLL.Dtos.ProductDto;
using App.UTIL.Abstractions.DTO.Response;

namespace App.BLL.Interfaces;

public interface IProductSvc
{
    Task<BaseResponse> GetProductListAsync(ProductFilter filter, CancellationToken ct = default);
    Task<BaseResponse> GetProductByIdAsync(int id, CancellationToken ct = default);
    Task<BaseResponse> GetProductBySlugAsync(string slug, CancellationToken ct = default);
    Task<BaseResponse> GetProductsByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    Task<BaseResponse> GetFiltersAsync(CancellationToken ct = default);
    Task<BaseResponse> GetRelatedProductsAsync(int productId, int limit = 6, CancellationToken ct = default);
    Task<BaseResponse> GetDescriptionByIdAsync(int id, CancellationToken ct = default);
    Task<BaseResponse> GetDescriptionBySlugAsync(string slug, CancellationToken ct = default);
}

