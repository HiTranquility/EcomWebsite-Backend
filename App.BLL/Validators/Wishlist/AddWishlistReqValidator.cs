using App.BLL.Dtos.WishlistDto.Requests;
using FluentValidation;

namespace App.BLL.Validators.Wishlist;

/// <summary>
/// Validator for AddWishlistReq
/// </summary>
public class AddWishlistReqValidator : AbstractValidator<AddWishlistReq>
{
    public AddWishlistReqValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0)
            .WithMessage("PRODUCT_ID_INVALID|ProductId must be greater than 0");
    }
}
