using App.BLL.Dtos.WishlistDto.Requests;
using App.BLL.Interfaces;
using App.UTIL.Abstractions.DTO.Response;
using App.UTIL.Extensions;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

/// <summary>
/// Wishlist Controller - Handles user wishlist operations
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/wishlist")]
public class WishlistController : ControllerBase
{
    private readonly IWishlistSvc _wishlistSvc;

    public WishlistController(IWishlistSvc wishlistSvc)
    {
        _wishlistSvc = wishlistSvc;
    }

    /// <summary>
    /// Get current user's wishlist
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWishlist(CancellationToken ct)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        BaseResponse rsp = await _wishlistSvc.GetItemsAsync(userId.Value, ct);
        return StatusCode(rsp.Status, rsp);
    }

    /// <summary>
    /// Add product to wishlist
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddToWishlist([FromBody] AddWishlistReq request, CancellationToken ct)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        // Validation handled by FluentValidation (AddWishlistReqValidator)
        BaseResponse rsp = await _wishlistSvc.AddItemAsync(userId.Value, request.ProductId, ct);
        return StatusCode(rsp.Status, rsp);
    }

    /// <summary>
    /// Remove item from wishlist
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteWishlistItem(int id, CancellationToken ct)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        BaseResponse rsp = await _wishlistSvc.RemoveItemAsync(userId.Value, id, ct);
        return StatusCode(rsp.Status, rsp);
    }

    /// <summary>
    /// Clear all items from wishlist
    /// </summary>
    [HttpDelete]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ClearWishlist(CancellationToken ct)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        BaseResponse rsp = await _wishlistSvc.ClearAllAsync(userId.Value, ct);
        return StatusCode(rsp.Status, rsp);
    }
}
