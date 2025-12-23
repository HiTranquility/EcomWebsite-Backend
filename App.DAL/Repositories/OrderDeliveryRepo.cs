using App.DAL.Interfaces;
using App.DAL.OrderModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.Repositories;

public class OrderDeliveryRepo : GenericRepo<EcomOrdersContext, OrderDelivery>, IOrderDeliveryRepo
{
    public OrderDeliveryRepo(EcomOrdersContext context) : base(context)
    {
    }
}


