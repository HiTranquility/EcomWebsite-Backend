using System.ComponentModel.DataAnnotations;
using App.UTIL.Abstractions.DTO.Request;

namespace App.BLL.Dtos.AuthDto.Requests;

public class ForgotPasswordReq : BaseRequest
{
    [Required(ErrorMessage = "EMAIL_REQUIRED|Email is required")]
    [EmailAddress(ErrorMessage = "EMAIL_INVALID|Email format is invalid")]
    public string Email { get; set; } = string.Empty;
}


