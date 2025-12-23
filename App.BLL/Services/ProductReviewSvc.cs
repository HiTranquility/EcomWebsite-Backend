using App.BLL.Dtos.ProductDto.Requests;
using App.BLL.Dtos.ProductDto.Results;
using App.DAL.ProductModels;
using App.DAL.Repositories;
using App.UTIL.Abstractions.BLL;
using App.UTIL.Abstractions.DTO.Response;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

using App.BLL.Interfaces;

namespace App.BLL.Services;

public class ProductReviewSvc : GenericSvc<ProductReviewRepo, ProductReview>, IProductReviewSvc
{
    private readonly ProductRepo _productRepo;

    public ProductReviewSvc(ProductReviewRepo repo, ProductRepo productRepo, IMapper mapper) : base(repo, mapper)
    {
        _productRepo = productRepo;
    }

    public async Task<BaseResponse> GetProductReviewsAsync(int productId, int? starRating = null, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        // Check if product exists
        var product = await _productRepo.ReadAsync(productId, ct);
        if (product == null)
        {
            rsp.SetError("PRODUCT_NOT_FOUND", "Product not found", "Product not found", 404);
            return rsp;
        }

        page = Math.Max(1, page);
        pageSize = Math.Max(1, Math.Min(100, pageSize));

        var query = _repo.All
            .AsNoTracking()
            .TagWith("ProductReviewSvc.GetProductReviewsAsync")
            .Where(r => r.ProductId == productId && r.DeletedAt == null);

        // Filter by star rating if provided
        if (starRating.HasValue && starRating.Value >= 1 && starRating.Value <= 5)
        {
            query = query.Where(r => r.StarRating == starRating.Value);
        }

        query = query.OrderByDescending(r => r.CreatedAt);

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<ProductReviewRes>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        // Get rating distribution for categories
        var ratingDistribution = await _repo.All
            .AsNoTracking()
            .Where(r => r.ProductId == productId && r.DeletedAt == null)
            .GroupBy(r => r.StarRating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var distribution = new Dictionary<int, int>();
        for (int i = 1; i <= 5; i++)
        {
            distribution[i] = 0;
        }
        foreach (var item in ratingDistribution)
        {
            if (item.Rating.HasValue && item.Rating.Value >= 1 && item.Rating.Value <= 5)
            {
                distribution[(int)item.Rating.Value] = item.Count;
            }
        }

        rsp.SetData(new
        {
            Total = total,
            Items = items,
            Page = page,
            PageSize = pageSize,
            RatingDistribution = distribution
        }, "Get product reviews successfully", 200);

        return rsp;
    }

    public async Task<BaseResponse> CreateReviewAsync(int userId, int productId, CreateReviewReq request, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        // Check if product exists
        var product = await _productRepo.ReadAsync(productId, ct);
        if (product == null)
        {
            rsp.SetError("PRODUCT_NOT_FOUND", "Product not found", "Product not found", 404);
            return rsp;
        }

        // Validate star rating
        if (request.StarRating.HasValue && (request.StarRating < 1 || request.StarRating > 5))
        {
            rsp.SetError("INVALID_RATING", "Star rating must be between 1 and 5", "Invalid star rating", 400);
            return rsp;
        }

        var review = _mapper.Map<ProductReview>(request);
        review.ProductId = productId;
        review.UserId = userId;
        review.CreatedAt = DateTime.UtcNow;
        review.UpdatedAt = DateTime.UtcNow;

        await _repo.CreateAsync(review, ct);

        var mapped = _mapper.Map<ProductReviewRes>(review);
        rsp.SetData(mapped, "Review created successfully", 201);

        return rsp;
    }

    public async Task<BaseResponse> UpdateReviewAsync(int userId, int reviewId, CreateReviewReq request, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        var review = await _repo.ReadAsync(reviewId, ct);
        if (review == null || review.DeletedAt != null)
        {
            rsp.SetError("REVIEW_NOT_FOUND", "Review not found", "Review not found", 404);
            return rsp;
        }

        // Check ownership
        if (review.UserId != userId)
        {
            rsp.SetError("FORBIDDEN", "You can only update your own reviews", "Forbidden", 403);
            return rsp;
        }

        // Validate star rating
        if (request.StarRating.HasValue && (request.StarRating < 1 || request.StarRating > 5))
        {
            rsp.SetError("INVALID_RATING", "Star rating must be between 1 and 5", "Invalid star rating", 400);
            return rsp;
        }

        // Update fields
        if (request.StarRating.HasValue)
            review.StarRating = request.StarRating;
        if (!string.IsNullOrWhiteSpace(request.Content))
            review.Content = request.Content.Trim();
        if (!string.IsNullOrWhiteSpace(request.FullName))
            review.FullName = request.FullName.Trim();
        if (!string.IsNullOrWhiteSpace(request.Email))
            review.Email = request.Email.Trim();
        review.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(review, ct);

        var mapped = _mapper.Map<ProductReviewRes>(review);
        rsp.SetData(mapped, "Review updated successfully", 200);

        return rsp;
    }

    public async Task<BaseResponse> DeleteReviewAsync(int userId, int reviewId, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        var review = await _repo.ReadAsync(reviewId, ct);
        if (review == null || review.DeletedAt != null)
        {
            rsp.SetError("REVIEW_NOT_FOUND", "Review not found", "Review not found", 404);
            return rsp;
        }

        // Check ownership
        if (review.UserId != userId)
        {
            rsp.SetError("FORBIDDEN", "You can only delete your own reviews", "Forbidden", 403);
            return rsp;
        }

        review.DeletedAt = DateTime.UtcNow;
        review.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(review, ct);

        rsp.SetData(null, "Review deleted successfully", 200);

        return rsp;
    }
}
