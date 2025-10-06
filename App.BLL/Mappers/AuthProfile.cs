
using App.BLL.Dtos.AuthDto.Requests;
using App.BLL.Dtos.AuthDto.Results;
using App.DAL.UserModels;
using AutoMapper;

namespace App.BLL.Mappers;

public class AuthProfile : Profile
{
    public AuthProfile()
    {
        CreateMap<RegisterReq, User>()
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email))
            .ForMember(d => d.PasswordHash, o => o.MapFrom(s => s.Password))
            .ForMember(d => d.Role, o => o.MapFrom(_ => "user"))
            .ForMember(d => d.IsActive, o => o.MapFrom(_ => true))
            .ForMember(d => d.IsSubscribe, o => o.MapFrom(_ => false))
            .ForMember(d => d.IsRemember, o => o.MapFrom(_ => false))
            .ForMember(d => d.CreatedAt, o => o.MapFrom(_ => DateTime.UtcNow))
            .ForMember(d => d.UpdatedAt, o => o.MapFrom(_ => DateTime.UtcNow));
        CreateMap<User, LoginReq>()
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email))
            .ForMember(d => d.Password, o => o.MapFrom(s => s.PasswordHash));
        CreateMap<User, ProfileRes>()
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email));
    }
}