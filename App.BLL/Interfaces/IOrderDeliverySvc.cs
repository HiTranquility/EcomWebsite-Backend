using App.UTIL.Abstractions.DTO.Response;

namespace App.BLL.Interfaces;

public interface IOrderDeliverySvc
{
    Task<BaseResponse> GetOrderDeliveryAsync(int userId, int orderId, CancellationToken ct = default);
}

