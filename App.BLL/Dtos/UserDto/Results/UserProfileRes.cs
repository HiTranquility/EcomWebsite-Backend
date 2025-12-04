using App.UTIL.Abstractions.DTO.Result;

namespace App.BLL.Dtos.UserDto.Results;

public class UserProfileRes : BaseResult
{
    public int Id { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public DateTime? Birthday { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Gender { get; set; }
    public bool? IsSubscribe { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

