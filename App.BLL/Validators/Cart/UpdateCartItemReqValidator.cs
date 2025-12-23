using App.BLL.Dtos.CartDto.Requests;
using FluentValidation;

namespace App.BLL.Validators.Cart;

/// <summary>
/// Validator for UpdateCartItemReq
/// </summary>
public class UpdateCartItemReqValidator : AbstractValidator<UpdateCartItemReq>
{
    public UpdateCartItemReqValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("QUANTITY_INVALID|Quantity must be greater than 0")
            .LessThanOrEqualTo(99)
            .WithMessage("QUANTITY_MAX|Quantity cannot exceed 99");
    }
}
