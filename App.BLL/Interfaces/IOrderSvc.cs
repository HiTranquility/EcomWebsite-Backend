using App.BLL.Dtos.OrderDto;
using App.BLL.Dtos.OrderDto.Requests;
using App.UTIL.Abstractions.DTO.Response;

namespace App.BLL.Interfaces;

public interface IOrderSvc
{
    Task<BaseResponse> CreateOrderAsync(int userId, CreateOrderReq request, CancellationToken ct = default);
    Task<BaseResponse> GetOrderDetailAsync(int userId, int orderId, CancellationToken ct = default);
    Task<BaseResponse> GetOrderListAsync(int userId, OrderFilter filter, CancellationToken ct = default);
}

