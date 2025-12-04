using App.UTIL.Abstractions.DTO.Request;
using System.ComponentModel.DataAnnotations;

namespace App.BLL.Dtos.AuthDto.Requests.Jwt;

public class LoginReq : BaseRequest
{
    [Required(ErrorMessage = "EMAIL_REQUIRED|Email is required")]
    [EmailAddress(ErrorMessage = "INVALID_EMAIL|Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "PASSWORD_REQUIRED|Password is required")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = false;
}