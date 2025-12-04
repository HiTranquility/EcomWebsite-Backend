using App.BLL.Dtos.WishlistDto.Results;
using App.BLL.Services;
using App.UTIL.Abstractions.DTO.Response;
using App.UTIL.Extensions;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/wishlist")]
public class WishlistController : ControllerBase
{
    private readonly WishlistSvc _wishlistSvc;

    public WishlistController(WishlistSvc wishlistSvc)
    {
        _wishlistSvc = wishlistSvc;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetWishlist(CancellationToken ct)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        BaseResponse rsp = await _wishlistSvc.GetWishlistAsync(userId.Value, ct);
        return StatusCode(rsp.Status, rsp);
    }

    public sealed record AddWishlistReq(int ProductId);

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddToWishlist([FromBody] AddWishlistReq request, CancellationToken ct)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        if (request.ProductId <= 0)
        {
            return BadRequest("PRODUCT_ID_INVALID|ProductId must be greater than 0");
        }

        BaseResponse rsp = await _wishlistSvc.AddAsync(userId.Value, request.ProductId, ct);
        return StatusCode(rsp.Status, rsp);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> RemoveFromWishlist(int id, CancellationToken ct)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        BaseResponse rsp = await _wishlistSvc.RemoveAsync(userId.Value, id, ct);
        return StatusCode(rsp.Status, rsp);
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> ClearWishlist(CancellationToken ct)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        BaseResponse rsp = await _wishlistSvc.ClearAsync(userId.Value, ct);
        return StatusCode(rsp.Status, rsp);
    }
}


