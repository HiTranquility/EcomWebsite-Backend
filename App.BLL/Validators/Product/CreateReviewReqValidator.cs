using App.BLL.Dtos.ProductDto.Requests;
using FluentValidation;

namespace App.BLL.Validators.Product;

/// <summary>
/// Validator for CreateReviewReq
/// </summary>
public class CreateReviewReqValidator : AbstractValidator<CreateReviewReq>
{
    public CreateReviewReqValidator()
    {
        RuleFor(x => x.StarRating)
            .NotNull()
            .WithMessage("STAR_RATING_REQUIRED|Star rating is required")
            .InclusiveBetween((uint)1, (uint)5)
            .WithMessage("STAR_RATING_INVALID|Star rating must be between 1 and 5");

        RuleFor(x => x.Content)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.Content))
            .WithMessage("CONTENT_MAX_LENGTH|Review content must not exceed 2000 characters");

        RuleFor(x => x.FullName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.FullName))
            .WithMessage("FULLNAME_MAX_LENGTH|Full name must not exceed 100 characters");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("EMAIL_INVALID|Email format is invalid");
    }
}
