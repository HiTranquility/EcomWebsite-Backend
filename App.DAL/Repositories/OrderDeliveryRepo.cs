using App.DAL.OrderModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.Repositories;

public class OrderDeliveryRepo : GenericRepo<EcomOrdersContext, OrderDelivery>
{
    public OrderDeliveryRepo(EcomOrdersContext context) : base(context)
    {
    }
}


