using App.BLL.Dtos.CartDto.Results;
using App.DAL.OrderModels;
using App.DAL.Repositories;
using App.UTIL.Abstractions.BLL;
using App.UTIL.Abstractions.DTO.Response;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace App.BLL.Services;

public class CartSvc : GenericSvc<CartRepo, Cart>
{
    private readonly ProductRepo _productRepo;

    public CartSvc(CartRepo repo, ProductRepo productRepo, IMapper mapper) : base(repo, mapper)
    {
        _productRepo = productRepo;
    }

    public Task<Cart?> GetActiveCartWithItemsAsync(int userId, int cartId, CancellationToken ct = default)
        => _repo.GetActiveCartWithItemsAsync(userId, cartId, ct);

    public async Task<bool> MarkCheckedOutAsync(int cartId, CancellationToken ct = default)
    {
        await _repo.MarkCheckedOutAsync(cartId, ct);
        return true;
    }

    public Task<Cart?> GetLatestActiveCartAsync(int userId, CancellationToken ct = default)
    {
        return _repo.All
            .AsNoTracking()
            .Include(c => c.CartItems)
            .Where(c => c.UserId == userId && (c.Status == null || c.Status == "active"))
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    private async Task<Cart?> GetLatestActiveCartForUpdateAsync(int userId, CancellationToken ct = default)
    {
        return await _repo.All
            .Include(c => c.CartItems)
            .Where(c => c.UserId == userId && (c.Status == null || c.Status == "active"))
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    private BaseResponse MapCartToResponse(Cart? cart)
    {
        var rsp = new BaseResponse();
        if (cart == null)
        {
            rsp.SetData((CartRes?)null);
            return rsp;
        }
        var mapped = _mapper.Map<CartRes>(cart);
        rsp.SetData(mapped);
        return rsp;
    }

    public async Task<BaseResponse> GetCartAsync(int userId, CancellationToken ct = default)
    {
        // Sync prices for items with price = 0 (old cart items)
        await _repo.SyncCartItemPricesAsync(userId, async (productId) =>
        {
            var product = await _productRepo.ReadAsync(productId, ct);
            if (product != null)
            {
                return product.LastestPrice ?? product.OriginalPrice;
            }
            return null;
        }, ct);
        
        // Get fresh cart after sync
        Cart? cart = await GetLatestActiveCartAsync(userId, ct);
        return MapCartToResponse(cart);
    }

    public async Task<BaseResponse> AddItemAsync(int userId, int productId, int? variantId, int quantity, decimal price, CancellationToken ct = default)
    {
        // If price is 0 or not provided, get price from product
        if (price <= 0)
        {
            var product = await _productRepo.ReadAsync(productId, ct);
            if (product != null)
            {
                price = product.LastestPrice ?? product.OriginalPrice ?? 0m;
            }
        }

        Cart cart = await _repo.AddItemAsync(userId, productId, variantId, quantity, price, ct);
        return MapCartToResponse(cart);
    }

    public async Task<BaseResponse> UpdateItemQuantityAsync(int userId, int cartItemId, int quantity, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();
        
        // Get cart item with tracking enabled to check if price needs to be synced
        var cart = await GetLatestActiveCartForUpdateAsync(userId, ct);
        if (cart == null)
        {
            rsp.SetError("CART_NOT_FOUND", "Cart not found", "No active cart found", 404);
            return rsp;
        }
        
        var cartItem = cart.CartItems?.FirstOrDefault(i => i.Id == cartItemId);
        if (cartItem == null)
        {
            rsp.SetError("CART_ITEM_NOT_FOUND", "Cart item not found", "The cart item you are trying to update does not exist", 404);
            return rsp;
        }
        
        // If price is 0, fetch from product before updating quantity
        if (cartItem.PriceAtTime <= 0)
        {
            var product = await _productRepo.ReadAsync(cartItem.ProductId, ct);
            if (product != null)
            {
                var newPrice = product.LastestPrice ?? product.OriginalPrice ?? 0m;
                if (newPrice > 0)
                {
                    cartItem.PriceAtTime = newPrice;
                    cartItem.Subtotal = newPrice * (cartItem.Quantity ?? 1);
                    cartItem.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
        
        // Now update quantity (this will also update subtotal)
        cartItem.Quantity = quantity;
        cartItem.Subtotal = cartItem.PriceAtTime * quantity;
        cartItem.UpdatedAt = DateTime.UtcNow;
        
        // Recalculate cart totals
        cart.TotalQuantity = cart.CartItems?.Sum(i => i.Quantity ?? 0) ?? 0;
        cart.TotalPrice = cart.CartItems?.Sum(i => i.Subtotal) ?? 0;
        cart.UpdatedAt = DateTime.UtcNow;
        
        await _repo.UpdateAsync(cart, ct);
        
        // Get fresh cart with no tracking for response
        cart = await GetLatestActiveCartAsync(userId, ct);
        return MapCartToResponse(cart);
    }

    public async Task<BaseResponse> RemoveItemAsync(int userId, int cartItemId, CancellationToken ct = default)
    {
        Cart? cart = await _repo.RemoveItemAsync(userId, cartItemId, ct);
        return MapCartToResponse(cart);
    }

    public async Task<BaseResponse> ClearCartAsync(int userId, CancellationToken ct = default)
    {
        Cart? cart = await _repo.ClearCartAsync(userId, ct);
        return MapCartToResponse(cart);
    }
}


