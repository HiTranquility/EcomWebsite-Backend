using App.BLL.Dtos.CartDto.Requests;
using FluentValidation;

namespace App.BLL.Validators.Cart;

/// <summary>
/// Validator for AddCartItemReq
/// </summary>
public class AddCartItemReqValidator : AbstractValidator<AddCartItemReq>
{
    public AddCartItemReqValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .WithMessage("PRODUCT_ID_INVALID|ProductId must be greater than 0");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .When(x => x.Quantity.HasValue)
            .WithMessage("QUANTITY_INVALID|Quantity must be greater than 0")
            .LessThanOrEqualTo(99)
            .When(x => x.Quantity.HasValue)
            .WithMessage("QUANTITY_MAX|Quantity cannot exceed 99");
    }
}
