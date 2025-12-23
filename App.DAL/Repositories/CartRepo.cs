using App.DAL.Interfaces;
using App.DAL.OrderModels;
using App.UTIL.Abstractions.DAL;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.Repositories;

public class CartRepo : GenericRepo<EcomOrdersContext, Cart>, ICartRepo
{
    public CartRepo(EcomOrdersContext context) : base(context)
    {
    }

    public Task<Cart?> GetActiveCartWithItemsAsync(int userId, int cartId, CancellationToken ct = default)
    {
        return _context.Carts
            .AsNoTracking()
            .Include(c => c.CartItems)
            .TagWith("CartRepo.GetActiveCartWithItemsAsync")
            .FirstOrDefaultAsync(c =>
                c.Id == cartId &&
                c.UserId == userId &&
                (c.Status == null || c.Status == "active"), ct);
    }

    public async Task MarkCheckedOutAsync(int cartId, CancellationToken ct = default)
    {
        await _context.Carts
            .Where(c => c.Id == cartId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(c => c.Status, "checked_out")
                .SetProperty(c => c.UpdatedAt, DateTime.UtcNow), ct);
    }

    public async Task<Cart> GetOrCreateActiveCartWithItemsAsync(int userId, CancellationToken ct = default)
    {
        Cart? cart = await _context.Carts
            .Include(c => c.CartItems)
            .TagWith("CartRepo.GetOrCreateActiveCartWithItemsAsync")
            .FirstOrDefaultAsync(c => c.UserId == userId && (c.Status == null || c.Status == "active"), ct);

        if (cart != null)
        {
            return cart;
        }

        cart = new Cart
        {
            UserId = userId,
            TotalPrice = 0,
            TotalQuantity = 0,
            Status = "active",
        };

        await _context.Carts.AddAsync(cart, ct);
        await _context.SaveChangesAsync(ct);

        await _context.Entry(cart).Collection(c => c.CartItems).LoadAsync(ct);
        return cart;
    }

    private static void RecalculateTotals(Cart cart)
    {
        cart.TotalQuantity = cart.CartItems.Sum(i => i.Quantity ?? 0);
        cart.TotalPrice = cart.CartItems.Sum(i => i.Subtotal);
        cart.UpdatedAt = DateTime.UtcNow;
    }

    public async Task<Cart> AddItemAsync(int userId, int productId, int? variantId, int quantity, decimal price, CancellationToken ct = default)
    {
        if (quantity <= 0) quantity = 1;

        Cart cart = await GetOrCreateActiveCartWithItemsAsync(userId, ct);

        CartItem? item = cart.CartItems.FirstOrDefault(i =>
            i.ProductId == productId && i.VariantId == variantId);

        if (item == null)
        {
            item = new CartItem
            {
                CartId = cart.Id,
                ProductId = productId,
                VariantId = variantId,
                Quantity = quantity,
                PriceAtTime = price,
                Subtotal = price * quantity,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            cart.CartItems.Add(item);
            _context.CartItems.Add(item);
        }
        else
        {
            item.Quantity = (item.Quantity ?? 0) + quantity;
            item.Subtotal = item.PriceAtTime * (item.Quantity ?? 0);
            item.UpdatedAt = DateTime.UtcNow;
        }

        RecalculateTotals(cart);
        await _context.SaveChangesAsync(ct);
        return cart;
    }

    public async Task<Cart?> UpdateItemQuantityAsync(int userId, int cartItemId, int quantity, CancellationToken ct = default)
    {
        if (quantity <= 0) quantity = 1;

        CartItem? item = await _context.CartItems
            .Include(i => i.Cart)
            .ThenInclude(c => c.CartItems)
            .TagWith("CartRepo.UpdateItemQuantityAsync")
            .FirstOrDefaultAsync(i => i.Id == cartItemId && i.Cart.UserId == userId && (i.Cart.Status == null || i.Cart.Status == "active"), ct);

        if (item == null)
        {
            return null;
        }

        // Ensure PriceAtTime is valid before calculating subtotal
        if (item.PriceAtTime <= 0)
        {
            // Price should have been synced in CartSvc before calling this method
            // But as a safety measure, use the existing subtotal divided by old quantity if available
            var oldQuantity = item.Quantity ?? 1;
            if (oldQuantity > 0 && item.Subtotal > 0)
            {
                item.PriceAtTime = item.Subtotal / oldQuantity;
            }
            else
            {
                // Fallback: set to 0, will be synced later
                item.PriceAtTime = 0;
            }
        }

        item.Quantity = quantity;
        item.Subtotal = item.PriceAtTime * quantity;
        item.UpdatedAt = DateTime.UtcNow;

        RecalculateTotals(item.Cart);
        await _context.SaveChangesAsync(ct);
        return item.Cart;
    }

    public async Task<Cart?> RemoveItemAsync(int userId, int cartItemId, CancellationToken ct = default)
    {
        CartItem? item = await _context.CartItems
            .Include(i => i.Cart)
            .ThenInclude(c => c.CartItems)
            .TagWith("CartRepo.RemoveItemAsync")
            .FirstOrDefaultAsync(i => i.Id == cartItemId && i.Cart.UserId == userId && (i.Cart.Status == null || i.Cart.Status == "active"), ct);

        if (item == null)
        {
            return null;
        }

        Cart cart = item.Cart;
        _context.CartItems.Remove(item);

        RecalculateTotals(cart);
        await _context.SaveChangesAsync(ct);
        return cart;
    }

    public async Task<Cart?> ClearCartAsync(int userId, CancellationToken ct = default)
    {
        Cart? cart = await _context.Carts
            .Include(c => c.CartItems)
            .TagWith("CartRepo.ClearCartAsync")
            .FirstOrDefaultAsync(c => c.UserId == userId && (c.Status == null || c.Status == "active"), ct);

        if (cart == null)
        {
            return null;
        }

        _context.CartItems.RemoveRange(cart.CartItems);
        cart.CartItems.Clear();

        RecalculateTotals(cart);
        await _context.SaveChangesAsync(ct);
        return cart;
    }

    public async Task<Cart?> SyncCartItemPricesAsync(int userId, Func<int, Task<decimal?>> getProductPriceAsync, CancellationToken ct = default)
    {
        Cart? cart = await _context.Carts
            .Include(c => c.CartItems)
            .TagWith("CartRepo.SyncCartItemPricesAsync")
            .FirstOrDefaultAsync(c => c.UserId == userId && (c.Status == null || c.Status == "active"), ct);

        if (cart == null || cart.CartItems.Count == 0)
        {
            return cart;
        }

        bool needsUpdate = false;
        foreach (var item in cart.CartItems.Where(i => i.PriceAtTime <= 0))
        {
            var newPrice = await getProductPriceAsync(item.ProductId);
            if (newPrice.HasValue && newPrice.Value > 0)
            {
                item.PriceAtTime = newPrice.Value;
                item.Subtotal = newPrice.Value * (item.Quantity ?? 1);
                item.UpdatedAt = DateTime.UtcNow;
                needsUpdate = true;
            }
        }

        if (needsUpdate)
        {
            RecalculateTotals(cart);
            await _context.SaveChangesAsync(ct);
        }

        return cart;
    }
}


