using App.BLL.Dtos.BlogDto.Requests;
using FluentValidation;

namespace App.BLL.Validators.Blog;

/// <summary>
/// Validator for CreateBlogCommentReq
/// </summary>
public class CreateBlogCommentReqValidator : AbstractValidator<CreateBlogCommentReq>
{
    public CreateBlogCommentReqValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("CONTENT_REQUIRED|Comment content is required")
            .MaximumLength(2000)
            .WithMessage("CONTENT_MAX_LENGTH|Comment must not exceed 2000 characters")
            .MinimumLength(2)
            .WithMessage("CONTENT_MIN_LENGTH|Comment must be at least 2 characters");

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
