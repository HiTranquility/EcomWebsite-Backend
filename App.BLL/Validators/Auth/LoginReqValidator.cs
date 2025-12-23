using App.BLL.Dtos.AuthDto.Requests.Jwt;
using FluentValidation;

namespace App.BLL.Validators.Auth;

/// <summary>
/// Validator for LoginReq
/// </summary>
public class LoginReqValidator : AbstractValidator<LoginReq>
{
    public LoginReqValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("EMAIL_REQUIRED|Email is required")
            .EmailAddress()
            .WithMessage("EMAIL_INVALID|Email format is invalid");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("PASSWORD_REQUIRED|Password is required")
            .MinimumLength(6)
            .WithMessage("PASSWORD_MIN_LENGTH|Password must be at least 6 characters");
    }
}
