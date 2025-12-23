using App.DAL.OrderModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.Interfaces;

public interface IOrderItemRepo : IGenericRepo<OrderItem>
{
}

