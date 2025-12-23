using App.BLL.Dtos.ProductDto.Requests;
using App.UTIL.Abstractions.DTO.Response;

namespace App.BLL.Interfaces;

public interface IProductReviewSvc
{
    Task<BaseResponse> GetProductReviewsAsync(int productId, int? starRating = null, int page = 1, int pageSize = 10, CancellationToken ct = default);
    Task<BaseResponse> CreateReviewAsync(int userId, int productId, CreateReviewReq request, CancellationToken ct = default);
    Task<BaseResponse> UpdateReviewAsync(int userId, int reviewId, CreateReviewReq request, CancellationToken ct = default);
    Task<BaseResponse> DeleteReviewAsync(int userId, int reviewId, CancellationToken ct = default);
}

