using App.BLL.Dtos.ProductDto.Requests;
using FluentValidation;

namespace App.BLL.Validators.Product;

/// <summary>
/// Validator for ProductBatchRequest
/// </summary>
public class ProductBatchRequestValidator : AbstractValidator<ProductBatchRequest>
{
    public ProductBatchRequestValidator()
    {
        RuleFor(x => x.Ids)
            .NotEmpty()
            .WithMessage("IDS_REQUIRED|Product IDs are required")
            .Must(ids => ids != null && ids.Length <= 50)
            .WithMessage("IDS_MAX_COUNT|Cannot request more than 50 products at once")
            .Must(ids => ids != null && ids.All(id => id > 0))
            .WithMessage("IDS_INVALID|All product IDs must be positive integers");
    }
}
