using System.ComponentModel.DataAnnotations;
using App.UTIL.Abstractions.DTO.Request;

namespace App.BLL.Dtos.AuthDto.Requests;

public class ResetPasswordReq : BaseRequest
{
    [Required(ErrorMessage = "EMAIL_REQUIRED|Email is required")]
    [EmailAddress(ErrorMessage = "EMAIL_INVALID|Email format is invalid")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "PASSWORD_REQUIRED|Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "PASSWORD_LENGTH_INVALID|Password length must be between 6 and 100 characters")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "TOKEN_REQUIRED|Reset token is required")]
    public string Token { get; set; } = string.Empty;
}


