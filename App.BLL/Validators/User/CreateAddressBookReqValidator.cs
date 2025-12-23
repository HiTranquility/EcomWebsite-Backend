using App.BLL.Dtos.UserDto.Requests;
using FluentValidation;

namespace App.BLL.Validators.User;

/// <summary>
/// Validator for CreateAddressBookReq
/// </summary>
public class CreateAddressBookReqValidator : AbstractValidator<CreateAddressBookReq>
{
    public CreateAddressBookReqValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("FULLNAME_REQUIRED|Full name is required")
            .MaximumLength(100)
            .WithMessage("FULLNAME_MAX_LENGTH|Full name must not exceed 100 characters");

        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage("ADDRESS_REQUIRED|Address is required")
            .MaximumLength(500)
            .WithMessage("ADDRESS_MAX_LENGTH|Address must not exceed 500 characters");

        RuleFor(x => x.Region)
            .NotEmpty()
            .WithMessage("REGION_REQUIRED|Region is required")
            .MaximumLength(100)
            .WithMessage("REGION_MAX_LENGTH|Region must not exceed 100 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("PHONE_NUMBER_REQUIRED|Phone number is required")
            .Matches(@"^(\+84|0)[0-9]{9,10}$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("PHONE_NUMBER_INVALID|Phone number format is invalid");
    }
}
