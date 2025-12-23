using App.BLL.Dtos.UserDto.Requests;
using App.UTIL.Abstractions.DTO.Response;

namespace App.BLL.Interfaces;

public interface IAddressBookSvc
{
    Task<BaseResponse> GetUserAddressesAsync(int userId, CancellationToken ct = default);
    Task<BaseResponse> GetAddressAsync(int userId, int addressId, CancellationToken ct = default);
    Task<BaseResponse> CreateAddressAsync(int userId, CreateAddressBookReq request, CancellationToken ct = default);
    Task<BaseResponse> UpdateAddressAsync(int userId, int addressId, CreateAddressBookReq request, CancellationToken ct = default);
    Task<BaseResponse> DeleteAddressAsync(int userId, int addressId, CancellationToken ct = default);
    Task<BaseResponse> SetDefaultAddressAsync(int userId, int addressId, CancellationToken ct = default);
}

