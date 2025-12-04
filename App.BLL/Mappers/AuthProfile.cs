using App.BLL.Dtos.AuthDto.Results;
using App.BLL.Dtos.AuthDto.Shares;
using App.DAL.UserModels;
using AutoMapper;

namespace App.BLL.Mappers;

public class AuthProfile : Profile
{
    public AuthProfile()
    {
        // User -> UserInfoItem
        CreateMap<User, UserInfoItem>()
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email))
            .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName))
            .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName))
            .ForMember(d => d.PhoneNumber, o => o.MapFrom(s => s.PhoneNumber))
            .ForMember(d => d.ImageUrl, o => o.MapFrom(s => s.ImageUrl));
    }
}

