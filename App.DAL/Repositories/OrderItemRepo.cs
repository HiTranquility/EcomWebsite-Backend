using App.DAL.Interfaces;
using App.DAL.OrderModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.Repositories;

public class OrderItemRepo : GenericRepo<EcomOrdersContext, OrderItem>, IOrderItemRepo
{
    public OrderItemRepo(EcomOrdersContext context) : base(context)
    {
    }
}


