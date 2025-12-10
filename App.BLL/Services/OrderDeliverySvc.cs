using App.BLL.Dtos.OrderDeliveryDto.Results;
using App.DAL.OrderModels;
using App.DAL.Repositories;
using App.UTIL.Abstractions.BLL;
using App.UTIL.Abstractions.DTO.Response;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace App.BLL.Services;

public class OrderDeliverySvc : GenericSvc<OrderDeliveryRepo, OrderDelivery>
{
    private readonly OrderRepo _orderRepo;

    public OrderDeliverySvc(
        OrderDeliveryRepo repo,
        OrderRepo orderRepo,
        IMapper mapper) : base(repo, mapper)
    {
        _orderRepo = orderRepo;
    }

    public async Task<BaseResponse> GetOrderDeliveryAsync(int userId, int orderId, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        var order = await _orderRepo.All
            .AsNoTracking()
            .Include(o => o.OrderDeliveries)
            .TagWith("OrderDeliverySvc.GetOrderDeliveryAsync")
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, ct);

        if (order == null)
        {
            rsp.SetError("ORDER_NOT_FOUND", "Order not found", "Order not found", 404);
            return rsp;
        }

        var delivery = order.OrderDeliveries
            .OrderByDescending(d => d.CreatedAt)
            .FirstOrDefault();

        // Nếu chưa có delivery cho đơn này, trả về 200 với payload = null
        if (delivery == null)
        {
            rsp.SetData((OrderDeliveryRes?)null, "No delivery information for this order yet", 200);
            return rsp;
        }

        var mapped = _mapper.Map<OrderDeliveryRes>(delivery);
        rsp.SetData(mapped, "Get order delivery successfully", 200);
        return rsp;
    }
}


