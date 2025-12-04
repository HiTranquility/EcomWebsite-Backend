using System;
using System.ComponentModel.DataAnnotations;
using App.UTIL.Abstractions.DTO.Request;

namespace App.BLL.Dtos.AuthDto.Requests.Jwt;

public class RegisterReq : BaseRequest
{
    [Required(ErrorMessage = "EMAIL_REQUIRED|Email is required"), EmailAddress(ErrorMessage = "EMAIL_INVALID|Email is invalid")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "FIRST_NAME_REQUIRED|First name is required")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "LAST_NAME_REQUIRED|Last name is required")]
    public string LastName { get; set; } = null!;

    [Required(ErrorMessage = "PASSWORD_REQUIRED|Password is required")]
    public string Password { get; set; } = null!;

    [Phone(ErrorMessage = "PHONE_NUMBER_INVALID|Phone number is invalid")]
    public string? PhoneNumber { get; set; }

    public string? Gender { get; set; }

    public DateTime? Birthday { get; set; }

    public bool? IsSubscribe { get; set; }
}