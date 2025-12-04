using App.BLL.Dtos.OrderDto.Requests;
using App.BLL.Dtos.OrderDto.Results;
using App.BLL.Dtos.OrderDto.Shares;
using App.DAL.OrderModels;
using AutoMapper;

namespace App.BLL.Mappers;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        // INPUT mapping: CreateOrderReq → Order (partial, business logic handled in service)
        CreateMap<CreateOrderReq, Order>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.CartId, o => o.Ignore())
            .ForMember(d => d.Status, o => o.MapFrom(_ => "pending"))
            .ForMember(d => d.PaymentStatus, o => o.MapFrom(_ => "pending"))
            .ForMember(d => d.TotalAmount, o => o.Ignore()) // Calculated from cart
            .ForMember(d => d.ShippingFee, o => o.MapFrom(s => s.ShippingFee ?? 0))
            .ForMember(d => d.DiscountAmount, o => o.MapFrom(s => s.DiscountAmount ?? 0))
            .ForMember(d => d.FinalPrice, o => o.Ignore()) // Calculated
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.Cart, o => o.Ignore())
            .ForMember(d => d.OrderDeliveries, o => o.Ignore())
            .ForMember(d => d.OrderItems, o => o.Ignore())
            .ForMember(d => d.Transactions, o => o.Ignore());

        // OUTPUT mappings
        CreateMap<OrderItem, OrderItemRes>();

        CreateMap<Order, OrderListItemRes>();

        CreateMap<Order, OrderDetailRes>()
            .ForMember(d => d.Items, o => o.MapFrom(s => s.OrderItems));
    }
}


