using App.BLL.Dtos.AuthDto.Requests.Jwt;
using App.BLL.Dtos.AuthDto.Requests.Google;
using App.BLL.Dtos.AuthDto.Requests.Facebook;
using App.UTIL.Abstractions.DTO.Response;

namespace App.BLL.Interfaces;

public interface IAuthSvc
{
    Task<BaseResponse> LoginAsync(LoginReq request, string? clientIp = null, CancellationToken ct = default);
    Task<BaseResponse> RegisterAsync(RegisterReq request, string? clientIp = null, CancellationToken ct = default);
    Task<BaseResponse> LogoutAsync(LogoutReq? request = null, string? refreshToken = null, string? clientIp = null, CancellationToken ct = default);
    Task<BaseResponse> RefreshTokenAsync(RefreshTokenReq request, string? clientIp = null, CancellationToken ct = default);
    Task<BaseResponse> GoogleCallbackAsync(CallbackReq request, string? clientIp = null, CancellationToken ct = default);
    Task<BaseResponse> GoogleIdTokenAsync(IdTokenReq request, string? clientIp = null, CancellationToken ct = default);
    Task<BaseResponse> FacebookAsync(AccessTokenReq request, string? clientIp = null, CancellationToken ct = default);
}

