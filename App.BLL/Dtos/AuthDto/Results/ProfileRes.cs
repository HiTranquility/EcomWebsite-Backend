using App.UTIL.Abstractions.DTO.Result;

namespace App.BLL.Dtos.AuthDto.Results;

public class ProfileRes : BaseResult
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Role { get; set; }
}


