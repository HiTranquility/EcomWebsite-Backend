using App.BLL.Dtos.UserDto.Requests;
using FluentValidation;

namespace App.BLL.Validators.User;

/// <summary>
/// Validator for UpdateUserProfileReq
/// </summary>
public class UpdateUserProfileReqValidator : AbstractValidator<UpdateUserProfileReq>
{
    public UpdateUserProfileReqValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.FirstName))
            .WithMessage("FIRST_NAME_MAX_LENGTH|First name must not exceed 50 characters");

        RuleFor(x => x.LastName)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.LastName))
            .WithMessage("LAST_NAME_MAX_LENGTH|Last name must not exceed 50 characters");

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
