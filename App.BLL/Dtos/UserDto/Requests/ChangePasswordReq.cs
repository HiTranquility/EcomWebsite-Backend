using App.UTIL.Abstractions.DTO.Request;

namespace App.BLL.Dtos.UserDto.Requests;

public class ChangePasswordReq : BaseRequest
{
    public string CurrentPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}

