using App.BLL.Dtos.OrderDeliveryDto.Results;
using App.DAL.OrderModels;
using AutoMapper;

namespace App.BLL.Mappers;

public class OrderDeliveryProfile : Profile
{
    public OrderDeliveryProfile()
    {
        CreateMap<OrderDelivery, OrderDeliveryRes>();
    }
}

