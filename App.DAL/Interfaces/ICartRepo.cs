using App.DAL.OrderModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.Interfaces;

public interface ICartRepo : IGenericRepo<Cart>
{
    Task<Cart?> GetActiveCartWithItemsAsync(int userId, int cartId, CancellationToken ct = default);
    Task MarkCheckedOutAsync(int cartId, CancellationToken ct = default);
    Task<Cart> GetOrCreateActiveCartWithItemsAsync(int userId, CancellationToken ct = default);
    Task<Cart> AddItemAsync(int userId, int productId, int? variantId, int quantity, decimal price, CancellationToken ct = default);
    Task<Cart?> UpdateItemQuantityAsync(int userId, int cartItemId, int quantity, CancellationToken ct = default);
    Task<Cart?> RemoveItemAsync(int userId, int cartItemId, CancellationToken ct = default);
    Task<Cart?> ClearCartAsync(int userId, CancellationToken ct = default);
    Task<Cart?> SyncCartItemPricesAsync(int userId, Func<int, Task<decimal?>> getProductPriceAsync, CancellationToken ct = default);
}

