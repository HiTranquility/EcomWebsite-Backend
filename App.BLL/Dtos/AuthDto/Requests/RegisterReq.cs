using System.ComponentModel.DataAnnotations;
using App.UTIL.Abstractions.DTO.Request;

namespace App.BLL.Dtos.AuthDto.Requests;

public class RegisterReq : BaseRequest
{
    [Required(ErrorMessage = "FIRSTNAME_REQUIRED|First name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "FIRSTNAME_LENGTH_INVALID|First name length must be between 2 and 50 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "LASTNAME_REQUIRED|Last name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "LASTNAME_LENGTH_INVALID|Last name length must be between 2 and 50 characters")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "EMAIL_REQUIRED|Email is required")]
    [EmailAddress(ErrorMessage = "EMAIL_INVALID|Email format is invalid")]
    [StringLength(100, ErrorMessage = "EMAIL_LENGTH_INVALID|Email length must not exceed 100 characters")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "PASSWORD_REQUIRED|Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "PASSWORD_LENGTH_INVALID|Password length must be between 6 and 100 characters")]
    public string Password { get; set; } = string.Empty;

    [Phone(ErrorMessage = "PHONE_INVALID|Phone number format is invalid")]
    [StringLength(20, ErrorMessage = "PHONE_LENGTH_INVALID|Phone number length must not exceed 20 characters")]
    public string? PhoneNumber { get; set; }
}