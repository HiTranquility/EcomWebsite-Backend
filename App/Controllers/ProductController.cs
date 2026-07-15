// ============================================================================
// Copyright (c) 2026 Nguyen Tan Phat (HiTranquility). All rights reserved.
// This source code is proprietary and confidential.
// Unauthorized copying, modification, or distribution is strictly prohibited.
// Contact: HiTranquility | CaPhiLe | Ba Chu Khanh
// ============================================================================
using App.BLL.Interfaces;
using Asp.Versioning;
using App.BLL.Dtos.ProductDto;
using App.BLL.Dtos.ProductDto.Requests;
using App.UTIL.Abstractions.DTO.Response;
using App.UTIL.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace App.Controllers;

/// <summary>
/// Product Controller - Handles product catalog, reviews, and tags
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products")]
public class ProductController : ControllerBase
{
    private readonly IProductSvc _productSvc;
    private readonly IProductReviewSvc _productReviewSvc;
    private readonly IProductTagSvc _productTagSvc;

    public ProductController(IProductSvc productSvc, IProductReviewSvc productReviewSvc, IProductTagSvc productTagSvc)
    {
        _productSvc = productSvc;
        _productReviewSvc = productReviewSvc;
        _productTagSvc = productTagSvc;
    }

    /// <summary>
    /// Get product list with filters and pagination
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductList([FromQuery] ProductFilter filter, CancellationToken ct)
    {
        var result = await _productSvc.GetProductListAsync(filter, ct);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Get available filter options (categories, brands, price ranges, etc.)
    /// </summary>
    [AllowAnonymous]
    [HttpGet("filters")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFilters(CancellationToken ct)
    {
        var result = await _productSvc.GetFiltersAsync(ct);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(int id, CancellationToken ct)
    {
        var result = await _productSvc.GetProductByIdAsync(id, ct);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Get product by slug
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{slug}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductBySlug(string slug, CancellationToken ct)
    {
        var result = await _productSvc.GetProductBySlugAsync(slug, ct);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Batch get products by IDs - reduces N+1 queries for cart/wishlist
    /// </summary>
    [AllowAnonymous]
    [HttpPost("batch")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProductsByIds([FromBody] ProductBatchRequest request, CancellationToken ct)
    {
        var result = await _productSvc.GetProductsByIdsAsync(request?.Ids ?? Array.Empty<int>(), ct);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Get related products
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id:int}/related")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRelatedProducts(int id, [FromQuery] int limit = 6, CancellationToken ct = default)
    {
        var result = await _productSvc.GetRelatedProductsAsync(id, limit, ct);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Get product description by ID
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id:int}/description")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDescriptionById(int id, CancellationToken ct = default)
    {
        var result = await _productSvc.GetDescriptionByIdAsync(id, ct);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Get product description by slug
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{slug}/description")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDescriptionBySlug(string slug, CancellationToken ct = default)
    {
        var result = await _productSvc.GetDescriptionBySlugAsync(slug, ct);
        return StatusCode(result.Status, result);
    }

    #region Product Reviews

    /// <summary>
    /// Get reviews for a product with optional star filter
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{productId:int}/reviews")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductReviews(int productId, [FromQuery] int? starRating = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await _productReviewSvc.GetProductReviewsAsync(productId, starRating, page, pageSize, ct);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Create a product review
    /// </summary>
    [Authorize]
    [HttpPost("{productId:int}/reviews")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateReview(int productId, [FromBody] CreateReviewReq request, CancellationToken ct = default)
    {
        // @author: CaPhiLe | Nguyen Tan Phat | HiTranquility
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _productReviewSvc.CreateReviewAsync(userId.Value, productId, request, ct);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Update a product review
    /// </summary>
    [Authorize]
    [HttpPut("reviews/{reviewId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateReview(int reviewId, [FromBody] CreateReviewReq request, CancellationToken ct = default)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _productReviewSvc.UpdateReviewAsync(userId.Value, reviewId, request, ct);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Delete a product review
    /// </summary>
    [Authorize]
    [HttpDelete("reviews/{reviewId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteReview(int reviewId, CancellationToken ct = default)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _productReviewSvc.DeleteReviewAsync(userId.Value, reviewId, ct);
        return StatusCode(result.Status, result);
    }

    #endregion

    #region Product Tags

    /// <summary>
    /// Get tags for a product (user-specific)
    /// </summary>
    [Authorize]
    [HttpGet("{productId:int}/tags")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProductTags(int productId, CancellationToken ct = default)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _productTagSvc.GetProductTagsAsync(userId, productId, ct);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Add a tag to a product
    /// </summary>
    [Authorize]
    [HttpPost("{productId:int}/tags")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateProductTag(int productId, [FromBody] CreateProductTagReq request, CancellationToken ct = default)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _productTagSvc.AddProductTagAsync(userId.Value, productId, request, ct);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Delete a product tag
    /// </summary>
    [Authorize]
    [HttpDelete("tags/{tagId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteProductTag(int tagId, CancellationToken ct = default)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _productTagSvc.DeleteProductTagAsync(userId.Value, tagId, ct);
        return StatusCode(result.Status, result);
    }

    #endregion
}