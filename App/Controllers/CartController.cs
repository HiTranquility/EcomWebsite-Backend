using App.BLL.Dtos.CartDto.Requests;
using App.BLL.Interfaces;
using App.UTIL.Abstractions.DTO.Response;
using App.UTIL.Extensions;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

/// <summary>
/// Cart Controller - Handles shopping cart operations
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/cart")]
public class CartController : ControllerBase
{
    private readonly ICartSvc _cartSvc;

    public CartController(ICartSvc cartSvc)
    {
        _cartSvc = cartSvc;
    }

    /// <summary>
    /// Get current user's cart
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCart(CancellationToken ct)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        BaseResponse rsp = await _cartSvc.GetCartAsync(userId.Value, ct);
        return StatusCode(rsp.Status, rsp);
    }

    /// <summary>
    /// Add item to cart
    /// </summary>
    [HttpPost("items")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemReq request, CancellationToken ct)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        int quantity = request.Quantity.GetValueOrDefault(1);
        BaseResponse rsp = await _cartSvc.AddItemAsync(userId.Value, request.ProductId, request.VariantId, quantity, 0, ct);
        return StatusCode(rsp.Status, rsp);
    }

    /// <summary>
    /// Update cart item quantity
    /// </summary>
    [HttpPut("items/{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] UpdateCartItemReq request, CancellationToken ct)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        BaseResponse rsp = await _cartSvc.UpdateItemQuantityAsync(userId.Value, id, request.Quantity, ct);
        return StatusCode(rsp.Status, rsp);
    }

    /// <summary>
    /// Remove item from cart
    /// </summary>
    [HttpDelete("items/{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RemoveItem(int id, CancellationToken ct)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        BaseResponse rsp = await _cartSvc.RemoveItemAsync(userId.Value, id, ct);
        return StatusCode(rsp.Status, rsp);
    }

    /// <summary>
    /// Clear all items from cart
    /// </summary>
    [HttpDelete("items")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ClearCart(CancellationToken ct)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        BaseResponse rsp = await _cartSvc.ClearCartAsync(userId.Value, ct);
        return StatusCode(rsp.Status, rsp);
    }
}
