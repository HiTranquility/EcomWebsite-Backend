using App.BLL.Dtos.WishlistDto.Results;
using App.DAL.UserModels;
using AutoMapper;

namespace App.BLL.Mappers;

public class WishlistProfile : Profile
{
    public WishlistProfile()
    {
        // Wishlist -> WishlistItemRes (mapping sẽ được xử lý trong service vì cần Product data)
        // Chỉ tạo map cơ bản, logic phức tạp sẽ ở service
        CreateMap<Wishlist, WishlistItemRes>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.ProductId, o => o.MapFrom(s => s.ProductId ?? 0))
            .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt));
    }
}

