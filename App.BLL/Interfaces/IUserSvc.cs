using App.BLL.Dtos.UserDto.Requests;
using App.UTIL.Abstractions.DTO.Response;

namespace App.BLL.Interfaces;

public interface IUserSvc
{
    Task<BaseResponse> GetUserProfileAsync(int userId, CancellationToken ct = default);
    Task<BaseResponse> UpdateUserProfileAsync(int userId, UpdateUserProfileReq request, CancellationToken ct = default);
    Task<BaseResponse> ChangePasswordAsync(int userId, ChangePasswordReq request, CancellationToken ct = default);
}

