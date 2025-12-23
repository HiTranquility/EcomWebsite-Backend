using App.BLL.Dtos.BlogDto;
using FluentValidation;

namespace App.BLL.Validators.Blog;

/// <summary>
/// Validator for BlogFilter query parameters
/// </summary>
public class BlogFilterValidator : AbstractValidator<BlogFilter>
{
    public BlogFilterValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("PAGE_INVALID|Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PAGE_SIZE_INVALID|PageSize must be between 1 and 100");

        RuleFor(x => x.Category)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.Category))
            .WithMessage("CATEGORY_MAX_LENGTH|Category must not exceed 50 characters");

        RuleFor(x => x.Author)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.Author))
            .WithMessage("AUTHOR_MAX_LENGTH|Author must not exceed 50 characters");

        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.Length <= 20)
            .WithMessage("TAGS_MAX_COUNT|Cannot filter by more than 20 tags");
    }
}
