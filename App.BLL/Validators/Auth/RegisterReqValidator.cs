using App.BLL.Dtos.AuthDto.Requests.Jwt;
using FluentValidation;

namespace App.BLL.Validators.Auth;

/// <summary>
/// Validator for RegisterReq
/// </summary>
public class RegisterReqValidator : AbstractValidator<RegisterReq>
{
    public RegisterReqValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("EMAIL_REQUIRED|Email is required")
            .EmailAddress()
            .WithMessage("EMAIL_INVALID|Email format is invalid");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("FIRST_NAME_REQUIRED|First name is required")
            .MaximumLength(50)
            .WithMessage("FIRST_NAME_MAX_LENGTH|First name must not exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("LAST_NAME_REQUIRED|Last name is required")
            .MaximumLength(50)
            .WithMessage("LAST_NAME_MAX_LENGTH|Last name must not exceed 50 characters");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("PASSWORD_REQUIRED|Password is required")
            .MinimumLength(8)
            .WithMessage("PASSWORD_MIN_LENGTH|Password must be at least 8 characters")
            .Matches(@"[A-Z]")
            .WithMessage("PASSWORD_UPPERCASE|Password must contain at least one uppercase letter")
            .Matches(@"[a-z]")
            .WithMessage("PASSWORD_LOWERCASE|Password must contain at least one lowercase letter")
            .Matches(@"[0-9]")
            .WithMessage("PASSWORD_DIGIT|Password must contain at least one digit");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^(\+84|0)[0-9]{9,10}$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("PHONE_NUMBER_INVALID|Phone number format is invalid");

        RuleFor(x => x.Gender)
            .Must(g => string.IsNullOrEmpty(g) || new[] { "male", "female", "other" }.Contains(g.ToLower()))
            .WithMessage("GENDER_INVALID|Gender must be male, female, or other");

        RuleFor(x => x.Birthday)
            .LessThan(DateTime.UtcNow.AddYears(-13))
            .When(x => x.Birthday.HasValue)
            .WithMessage("BIRTHDAY_AGE_INVALID|User must be at least 13 years old");
    }
}
