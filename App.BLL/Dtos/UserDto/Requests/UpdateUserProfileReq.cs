using App.UTIL.Abstractions.DTO.Request;

namespace App.BLL.Dtos.UserDto.Requests;

public class UpdateUserProfileReq : BaseRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? Birthday { get; set; }
    public string? Gender { get; set; }
    public bool? IsSubscribe { get; set; }
}

