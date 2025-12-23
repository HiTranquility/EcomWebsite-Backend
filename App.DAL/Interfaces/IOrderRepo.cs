using App.DAL.OrderModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.Interfaces;

public interface IOrderRepo : IGenericRepo<Order>
{
    Task<Order> CreateOrderWithItemsAsync(Order order, List<OrderItem> items, OrderDelivery? delivery = null, CancellationToken ct = default);
    Task<Order?> GetOrderWithDetailsAsync(int orderId, int userId, CancellationToken ct = default);
}

