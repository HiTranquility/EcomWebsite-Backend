using App.BLL.Services;
using Asp.Versioning;
using App.BLL.Dtos.ProductDto;
using App.BLL.Dtos.ProductDto.Requests;
using App.UTIL.Abstractions.DTO.Response;
using App.UTIL.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace App.Controllers;
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products")]
public class ProductController : ControllerBase
{
    private readonly ProductSvc _productSvc;
    private readonly ProductReviewSvc _productReviewSvc;
    private readonly ProductTagSvc _productTagSvc;

    public ProductController(ProductSvc productSvc, ProductReviewSvc productReviewSvc, ProductTagSvc productTagSvc)
    {
        _productSvc = productSvc;
        _productReviewSvc = productReviewSvc;
        _productTagSvc = productTagSvc;
    }
    
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetProductList([FromQuery] ProductFilter filter, CancellationToken ct)
    {
        var result = await _productSvc.GetProductListAsync(filter, ct);
        return StatusCode(result.Status, result);
    }
    [AllowAnonymous]
    [HttpGet("filter")]
    public async Task<IActionResult> GetProductFilter(CancellationToken ct)
    {
        var result = await _productSvc.GetProductFilterAsync(ct);
        return StatusCode(result.Status, result);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProductDetail(int id, CancellationToken ct)
    {
        var result = await _productSvc.GetProductInformationByIdAsync(id, ct);
        return StatusCode(result.Status, result);
    }

    [AllowAnonymous]
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetProductDetailBySlug(string slug, CancellationToken ct)
    {
        var result = await _productSvc.GetProductInformationBySlugAsync(slug, ct);
        return StatusCode(result.Status, result);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}/related")]
    public async Task<IActionResult> GetRelatedProducts(int id, [FromQuery] int limit = 6, CancellationToken ct = default)
    {
        var result = await _productSvc.GetRelatedProductsAsync(id, limit, ct);
        return StatusCode(result.Status, result);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}/description")]
    public async Task<IActionResult> GetProductDescriptionById(int id, CancellationToken ct = default)
    {
        var result = await _productSvc.GetProductDescriptionByIdAsync(id, ct);
        return StatusCode(result.Status, result);
    }

    [AllowAnonymous]
    [HttpGet("{slug}/description")]
    public async Task<IActionResult> GetProductDescriptionBySlug(string slug, CancellationToken ct = default)
    {
        var result = await _productSvc.GetProductDescriptionBySlugAsync(slug, ct);
        return StatusCode(result.Status, result);
    }

    // Product Reviews endpoints
    [AllowAnonymous]
    [HttpGet("{productId:int}/reviews")]
    public async Task<IActionResult> GetProductReviews(int productId, [FromQuery] int? starRating = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await _productReviewSvc.GetProductReviewsAsync(productId, starRating, page, pageSize, ct);
        return StatusCode(result.Status, result);
    }

    [Authorize]
    [HttpPost("{productId:int}/reviews")]
    public async Task<IActionResult> CreateReview(int productId, [FromBody] CreateReviewReq request, CancellationToken ct = default)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _productReviewSvc.CreateReviewAsync(userId.Value, productId, request, ct);
        return StatusCode(result.Status, result);
    }

    [Authorize]
    [HttpPut("reviews/{reviewId:int}")]
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

    [Authorize]
    [HttpDelete("reviews/{reviewId:int}")]
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

    // Product Tags endpoints
    [Authorize]
    [HttpGet("{productId:int}/tags")]
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

    [Authorize]
    [HttpPost("{productId:int}/tags")]
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

    [Authorize]
    [HttpDelete("tags/{tagId:int}")]
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
}