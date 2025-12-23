using App.BLL.Dtos.UserDto.Requests;
using FluentValidation;

namespace App.BLL.Validators.User;

/// <summary>
/// Validator for ChangePasswordReq
/// </summary>
public class ChangePasswordReqValidator : AbstractValidator<ChangePasswordReq>
{
    public ChangePasswordReqValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("CURRENT_PASSWORD_REQUIRED|Current password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("NEW_PASSWORD_REQUIRED|New password is required")
            .MinimumLength(8)
            .WithMessage("PASSWORD_MIN_LENGTH|Password must be at least 8 characters")
            .Matches(@"[A-Z]")
            .WithMessage("PASSWORD_UPPERCASE|Password must contain at least one uppercase letter")
            .Matches(@"[a-z]")
            .WithMessage("PASSWORD_LOWERCASE|Password must contain at least one lowercase letter")
            .Matches(@"[0-9]")
            .WithMessage("PASSWORD_DIGIT|Password must contain at least one digit")
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("PASSWORD_SAME|New password must be different from current password");
    }
}
