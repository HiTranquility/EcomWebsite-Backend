using FluentValidation;
using App.BLL.Dtos.ProductDto;

namespace App.BLL.Validators.Product;

/// <summary>
/// Validator for ProductFilter query parameters
/// </summary>
public class ProductFilterValidator : AbstractValidator<ProductFilter>
{
    private static readonly HashSet<string> AllowedSizes = new(StringComparer.OrdinalIgnoreCase)
    {
        "XS", "S", "M", "L", "XL", "XXL", "XXXL"
    };

    private static readonly HashSet<string> AllowedSorts = new(StringComparer.OrdinalIgnoreCase)
    {
        "newest",
        "oldest",
        "priceasc",
        "pricedesc",
        "popular",
        "rating"
    };

    public ProductFilterValidator()
    {
        // Cross-field validation: PriceMin <= PriceMax
        RuleFor(x => x)
            .Must(x => !x.PriceMin.HasValue || !x.PriceMax.HasValue || x.PriceMin <= x.PriceMax)
            .WithMessage("PRICE_RANGE_INVALID|PriceMin must be less than or equal to PriceMax");

        // Size validation
        RuleForEach(x => x.Sizes)
            .Must(size => AllowedSizes.Contains(size))
            .When(x => x.Sizes != null && x.Sizes.Any())
            .OverridePropertyName("Sizes")
            .WithMessage("SIZE_INVALID|Size must be one of: XS, S, M, L, XL, XXL, XXXL");

        // Rating validation
        RuleFor(x => x.MinRating)
            .InclusiveBetween(1, 5)
            .When(x => x.MinRating.HasValue)
            .WithMessage("RATING_INVALID|Rating must be between 1 and 5");

        // Sort validation
        RuleFor(x => x.Sort)
            .Must(sort => string.IsNullOrWhiteSpace(sort) || AllowedSorts.Contains(sort.Trim().ToLowerInvariant()))
            .WithMessage("SORT_INVALID|Sort must be one of: newest, oldest, priceAsc, priceDesc, popular, rating");

        // Price validation
        RuleFor(x => x.PriceMin)
            .GreaterThanOrEqualTo(0)
            .When(x => x.PriceMin.HasValue)
            .WithMessage("PRICE_MIN_INVALID|PriceMin must be >= 0");

        RuleFor(x => x.PriceMax)
            .GreaterThanOrEqualTo(0)
            .When(x => x.PriceMax.HasValue)
            .WithMessage("PRICE_MAX_INVALID|PriceMax must be >= 0");

        // Pagination validation (int, not nullable - from BaseFilter)
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("PAGE_INVALID|Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PAGE_SIZE_INVALID|PageSize must be between 1 and 100");
    }
}
