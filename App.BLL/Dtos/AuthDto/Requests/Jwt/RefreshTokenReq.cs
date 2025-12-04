using App.UTIL.Abstractions.DTO.Request;
using System.ComponentModel.DataAnnotations;

namespace App.BLL.Dtos.AuthDto.Requests.Jwt;

public class RefreshTokenReq : BaseRequest
{
    [Required(ErrorMessage = "REFRESH_TOKEN_REQUIRED|Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;
}