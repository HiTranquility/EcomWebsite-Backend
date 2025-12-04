using FluentValidation;
using App.BLL.Dtos.ProductDto;

namespace App.BLL.Validators;

public class ProductFilterValidator : AbstractValidator<ProductFilter>
{
    // Danh sách sizes hợp lệ ví dụ
    private readonly List<string> allowedSizes = new() { "S", "M", "L", "XL" };

    public ProductFilterValidator()
    {
        // Rule cross-field: PriceMin <= PriceMax
        RuleFor(f => f)
            .Must(f => !f.PriceMin.HasValue || !f.PriceMax.HasValue || f.PriceMin <= f.PriceMax)
            .WithMessage("PRICE_RANGE_INVALID|PriceMin must be less than or equal to PriceMax");

        // Rule Size list
        RuleForEach(f => f.Sizes)
            .Must(s => allowedSizes.Contains(s))
            .OverridePropertyName("Sizes")
            .When(f => f.Sizes != null)
            .WithMessage("INVALID_SIZE|Size must be one of S, M, L, XL");

        // Rule MinRating
        RuleFor(f => f.MinRating)
            .InclusiveBetween(1, 5)
            .When(f => f.MinRating.HasValue)
            .WithMessage("RATING_MIN_INVALID|Rating must be between 1 and 5");

        // Conditional rule example
        RuleFor(f => f.PriceMin)
            .GreaterThanOrEqualTo(0)
            .When(f => f.IsFlashsale == true)
            .WithMessage("PRICE_MIN_INVALID|PriceMin must be >= 0 for flashsale items");
    }
}
