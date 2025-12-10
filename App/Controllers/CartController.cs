using App.BLL.Services;
using App.UTIL.Abstractions.DTO.Response;
using App.UTIL.Extensions;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/cart")]
public class CartController : ControllerBase
{
    private readonly CartSvc _cartSvc;

    public CartController(CartSvc cartSvc)
    {
        _cartSvc = cartSvc;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetCart(CancellationToken ct)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        BaseResponse rsp = await _cartSvc.GetCartAsync(userId.Value, ct);
        return StatusCode(rsp.Status, rsp);
    }

    public sealed record AddCartItemReq(int ProductId, int? VariantId, int? Quantity);

    [HttpPost("items")]
    [Authorize]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemReq request, CancellationToken ct)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        // Validation moved to service layer
        int quantity = request.Quantity.GetValueOrDefault(1);
        BaseResponse rsp = await _cartSvc.AddItemAsync(userId.Value, request.ProductId, request.VariantId, quantity, 0, ct);
        return StatusCode(rsp.Status, rsp);
    }

    public sealed record UpdateCartItemReq(int Quantity);

    [HttpPut("items/{id:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] UpdateCartItemReq request, CancellationToken ct)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        // Validation moved to service layer
        BaseResponse rsp = await _cartSvc.UpdateItemQuantityAsync(userId.Value, id, request.Quantity, ct);
        return StatusCode(rsp.Status, rsp);
    }

    [HttpDelete("items/{id:int}")]
    [Authorize]
    public async Task<IActionResult> RemoveItem(int id, CancellationToken ct)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        BaseResponse rsp = await _cartSvc.RemoveItemAsync(userId.Value, id, ct);
        return StatusCode(rsp.Status, rsp);
    }

    [HttpDelete("items")]
    [Authorize]
    public async Task<IActionResult> ClearCart(CancellationToken ct)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        BaseResponse rsp = await _cartSvc.ClearCartAsync(userId.Value, ct);
        return StatusCode(rsp.Status, rsp);
    }
}


