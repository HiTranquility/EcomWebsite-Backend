using App.DAL.OrderModels;
using App.UTIL.Abstractions.DAL;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.Repositories;

public class OrderRepo : GenericRepo<EcomOrdersContext, Order>
{
    public OrderRepo(EcomOrdersContext context) : base(context)
    {
    }

    public async Task<Order> CreateOrderWithItemsAsync(Order order, List<OrderItem> items, CancellationToken ct = default)
    {
        await using var tx = await _context.Database.BeginTransactionAsync(ct);

        await _context.Orders.AddAsync(order, ct);
        await _context.SaveChangesAsync(ct);

        if (items.Count > 0)
        {
            foreach (var item in items)
            {
                item.OrderId = order.Id;
            }

            await _context.OrderItems.AddRangeAsync(items, ct);
            await _context.SaveChangesAsync(ct);
        }

        await tx.CommitAsync(ct);
        return order;
    }

    public Task<Order?> GetOrderWithDetailsAsync(int orderId, int userId, CancellationToken ct = default)
    {
        return _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
            .Include(o => o.OrderDeliveries)
            .Include(o => o.Transactions)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, ct);
    }
}


