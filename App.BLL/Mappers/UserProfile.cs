using App.BLL.Dtos.UserDto.Requests;
using App.BLL.Dtos.UserDto.Results;
using App.DAL.UserModels;
using AutoMapper;

namespace App.BLL.Mappers;

public class UserProfile : Profile
{
    public UserProfile()
    {
        // User -> UserProfileRes
        CreateMap<User, UserProfileRes>();

        // AddressBook mappings
        CreateMap<CreateAddressBookReq, AddressBook>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.FullName, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.FullName) ? null : s.FullName.Trim()))
            .ForMember(d => d.Address, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.Address) ? null : s.Address.Trim()))
            .ForMember(d => d.Region, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.Region) ? null : s.Region.Trim()))
            .ForMember(d => d.PhoneNumber, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.PhoneNumber) ? null : s.PhoneNumber.Trim()))
            .ForMember(d => d.IsBillingAddress, o => o.MapFrom(s => s.IsBillingAddress ?? false))
            .ForMember(d => d.IsDefaultAddress, o => o.MapFrom(s => s.IsDefaultAddress ?? false))
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.DeletedAt, o => o.Ignore())
            .ForMember(d => d.User, o => o.Ignore());

        CreateMap<AddressBook, AddressBookRes>();
    }
}

