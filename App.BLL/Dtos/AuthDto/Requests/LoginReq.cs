using App.UTIL.Abstractions.DTO.Request;

namespace App.BLL.Dtos.AuthDto.Requests;

public class LoginReq : BaseRequest
{
    public string? Email { get; set; } = string.Empty;
    public string? Password { get; set; } = string.Empty;
}