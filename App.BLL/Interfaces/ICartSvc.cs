using App.DAL.OrderModels;
using App.UTIL.Abstractions.DTO.Response;

namespace App.BLL.Interfaces;

public interface ICartSvc
{
    Task<Cart?> GetActiveCartWithItemsAsync(int userId, int cartId, CancellationToken ct = default);
    Task<bool> MarkCheckedOutAsync(int cartId, CancellationToken ct = default);
    Task<BaseResponse> GetCartAsync(int userId, CancellationToken ct = default);
    Task<BaseResponse> AddItemAsync(int userId, int productId, int? variantId, int quantity, decimal price, CancellationToken ct = default);
    Task<BaseResponse> UpdateItemQuantityAsync(int userId, int cartItemId, int quantity, CancellationToken ct = default);
    Task<BaseResponse> RemoveItemAsync(int userId, int cartItemId, CancellationToken ct = default);
    Task<BaseResponse> ClearCartAsync(int userId, CancellationToken ct = default);
}

