using App.UTIL.Abstractions.DTO.Response;

namespace App.BLL.Interfaces;

public interface IWishlistSvc
{
    Task<BaseResponse> GetItemsAsync(int userId, CancellationToken ct = default);
    Task<BaseResponse> AddItemAsync(int userId, int productId, CancellationToken ct = default);
    Task<BaseResponse> RemoveItemAsync(int userId, int id, CancellationToken ct = default);
    Task<BaseResponse> ClearAllAsync(int userId, CancellationToken ct = default);
}

