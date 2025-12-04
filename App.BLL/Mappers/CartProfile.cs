using App.BLL.Dtos.CartDto.Results;
using App.BLL.Dtos.CartDto.Shares;
using App.DAL.OrderModels;
using AutoMapper;

namespace App.BLL.Mappers;

public class CartProfile : Profile
{
    public CartProfile()
    {
        CreateMap<CartItem, CartItemRes>();

        CreateMap<Cart, CartRes>()
            .ForMember(d => d.TotalPrice, o => o.MapFrom(s => s.TotalPrice ?? 0))
            .ForMember(d => d.TotalQuantity, o => o.MapFrom(s => s.TotalQuantity ?? 0))
            .ForMember(d => d.Items, o => o.MapFrom(s => s.CartItems));
    }
}

