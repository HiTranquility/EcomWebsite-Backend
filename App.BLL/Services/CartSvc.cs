using App.BLL.Dtos.CartDto.Results;
using App.DAL.OrderModels;
using App.DAL.Repositories;
using App.UTIL.Abstractions.BLL;
using App.UTIL.Abstractions.DTO.Response;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using App.BLL.Interfaces;

namespace App.BLL.Services;

public class CartSvc : GenericSvc<CartRepo, Cart>, ICartSvc
{
    private readonly ProductRepo _productRepo;
    private readonly ILogger<CartSvc> _logger;

    public CartSvc(CartRepo repo, ProductRepo productRepo, IMapper mapper, ILogger<CartSvc> logger) : base(repo, mapper)
    {
        _productRepo = productRepo;
        _logger = logger;
    }

    public Task<Cart?> GetActiveCartWithItemsAsync(int userId, int cartId, CancellationToken ct = default)
        => _repo.GetActiveCartWithItemsAsync(userId, cartId, ct);

    public async Task<bool> MarkCheckedOutAsync(int cartId, CancellationToken ct = default)
    {
        await _repo.MarkCheckedOutAsync(cartId, ct);
        return true;
    }

    public async Task<BaseResponse> GetCartAsync(int userId, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        try
        {
        // Sync prices for items with price = 0 (old cart items)
        await _repo.SyncCartItemPricesAsync(userId, async (productId) =>
        {
            var product = await _productRepo.All
                .AsNoTracking()
                .TagWith("CartSvc.GetCartAsync.SyncPrices")
                .FirstOrDefaultAsync(p => p.Id == productId && p.DeletedAt == null, ct);
            if (product != null)
            {
                return product.LatestPrice ?? product.OriginalPrice;
            }
            return null;
        }, ct);
        
        // Get fresh cart after sync
        var cart = await _repo.All
            .AsNoTracking()
            .Include(c => c.CartItems)
            .TagWith("CartSvc.GetCartAsync")
            .Where(c => c.UserId == userId && (c.Status == null || c.Status == "active"))
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (cart == null)
        {
            rsp.SetData((CartRes?)null, "Cart is empty", 200);
            return rsp;
        }

        var mapped = _mapper.Map<CartRes>(cart);
        rsp.SetData(mapped, "Get cart successfully", 200);
        return rsp;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetCartAsync failed for user {UserId}", userId);
            rsp.SetError("CART_FETCH_FAILED", "Unable to fetch cart", ex.Message, 500);
            return rsp;
        }
    }

    public async Task<BaseResponse> AddItemAsync(int userId, int productId, int? variantId, int quantity, decimal price, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        // Validation
        if (productId <= 0)
        {
            rsp.SetError("PRODUCT_ID_INVALID", "Invalid Product ID", "ProductId must be greater than 0", 400);
            return rsp;
        }

        // Normalize quantity
        if (quantity <= 0) quantity = 1;
        if (quantity > 1000)
        {
            rsp.SetError("QUANTITY_INVALID", "Invalid Quantity", "Quantity cannot exceed 1000", 400);
            return rsp;
        }

        // If price is 0 or not provided, get price from product
        if (price <= 0)
        {
            var product = await _productRepo.All
                .AsNoTracking()
                .TagWith("CartSvc.AddItemAsync.GetProduct")
                .FirstOrDefaultAsync(p => p.Id == productId && p.DeletedAt == null, ct);

            if (product == null)
            {
                rsp.SetError("PRODUCT_NOT_FOUND", "Product Not Found", "The product you are trying to add does not exist", 404);
                return rsp;
            }
            price = product.LatestPrice ?? product.OriginalPrice ?? 0m;
            if (price <= 0)
            {
                rsp.SetError("PRODUCT_PRICE_INVALID", "Invalid Product Price", "Product price is not available", 400);
                return rsp;
            }
        }

        var cart = await _repo.AddItemAsync(userId, productId, variantId, quantity, price, ct);
        var mapped = _mapper.Map<CartRes>(cart);
        rsp.SetData(mapped, "Item added to cart successfully", 200);
        return rsp;
    }

    public async Task<BaseResponse> UpdateItemQuantityAsync(int userId, int cartItemId, int quantity, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        // Validation
        if (quantity <= 0)
        {
            rsp.SetError("QUANTITY_INVALID", "Invalid Quantity", "Quantity must be greater than 0", 400);
            return rsp;
        }
        if (quantity > 1000)
        {
            rsp.SetError("QUANTITY_INVALID", "Invalid Quantity", "Quantity cannot exceed 1000", 400);
            return rsp;
        }
        
        // Get cart item with tracking enabled
        var cart = await _repo.All
            .Include(c => c.CartItems)
            .TagWith("CartSvc.UpdateItemQuantityAsync")
            .Where(c => c.UserId == userId && (c.Status == null || c.Status == "active"))
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(ct);

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
            var product = await _productRepo.All
                .AsNoTracking()
                .TagWith("CartSvc.UpdateItemQuantityAsync.GetProduct")
                .FirstOrDefaultAsync(p => p.Id == cartItem.ProductId && p.DeletedAt == null, ct);

            if (product != null)
            {
                var newPrice = product.LatestPrice ?? product.OriginalPrice ?? 0m;
                if (newPrice > 0)
                {
                    cartItem.PriceAtTime = newPrice;
                    cartItem.Subtotal = newPrice * (cartItem.Quantity ?? 1);
                    cartItem.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
        
        // Update quantity
        cartItem.Quantity = quantity;
        cartItem.Subtotal = cartItem.PriceAtTime * quantity;
        cartItem.UpdatedAt = DateTime.UtcNow;
        
        // Recalculate cart totals
        cart.TotalQuantity = cart.CartItems?.Sum(i => i.Quantity ?? 0) ?? 0;
        cart.TotalPrice = cart.CartItems?.Sum(i => i.Subtotal) ?? 0;
        cart.UpdatedAt = DateTime.UtcNow;
        
        await _repo.UpdateAsync(cart, ct);
        
        // Get fresh cart with no tracking for response
        cart = await _repo.All
            .AsNoTracking()
            .Include(c => c.CartItems)
            .TagWith("CartSvc.UpdateItemQuantityAsync.GetResponse")
            .Where(c => c.UserId == userId && (c.Status == null || c.Status == "active"))
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(ct);

        var mapped = _mapper.Map<CartRes>(cart);
        rsp.SetData(mapped, "Cart item updated successfully", 200);
        return rsp;
    }

    public async Task<BaseResponse> RemoveItemAsync(int userId, int cartItemId, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        var cart = await _repo.RemoveItemAsync(userId, cartItemId, ct);
        if (cart == null)
        {
            rsp.SetError("CART_ITEM_NOT_FOUND", "Cart item not found", "The cart item you are trying to remove does not exist", 404);
            return rsp;
        }

        var mapped = _mapper.Map<CartRes>(cart);
        rsp.SetData(mapped, "Item removed from cart successfully", 200);
        return rsp;
    }

    public async Task<BaseResponse> ClearCartAsync(int userId, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        var cart = await _repo.ClearCartAsync(userId, ct);
        if (cart == null)
        {
            rsp.SetData((CartRes?)null, "Cart is already empty", 200);
            return rsp;
        }

        var mapped = _mapper.Map<CartRes>(cart);
        rsp.SetData(mapped, "Cart cleared successfully", 200);
        return rsp;
    }
}


