using App.BLL.Dtos.OrderDto.Requests;
using FluentValidation;

namespace App.BLL.Validators.Order;

/// <summary>
/// Validator for CreateOrderReq
/// </summary>
public class CreateOrderReqValidator : AbstractValidator<CreateOrderReq>
{
    private static readonly HashSet<string> AllowedDeliveryTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "standard",
        "fast"
    };

    private static readonly HashSet<string> AllowedPaymentMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "COD",
        "VNPAY",
        "MOMO",
        "STRIPE"
    };

    public CreateOrderReqValidator()
    {
        RuleFor(x => x.CartId)
            .GreaterThan(0)
            .WithMessage("CART_ID_INVALID|CartId must be a positive integer");

        RuleFor(x => x.DeliveryType)
            .Must(type => string.IsNullOrWhiteSpace(type) || AllowedDeliveryTypes.Contains(type.Trim()))
            .WithMessage("DELIVERY_TYPE_INVALID|DeliveryType must be one of: standard, fast");

        RuleFor(x => x.ShippingFee)
            .GreaterThanOrEqualTo(0)
            .When(x => x.ShippingFee.HasValue)
            .WithMessage("SHIPPING_FEE_INVALID|ShippingFee must be >= 0");

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.DiscountAmount.HasValue)
            .WithMessage("DISCOUNT_AMOUNT_INVALID|DiscountAmount must be >= 0");

        RuleFor(x => x.PaymentMethod)
            .Must(method => string.IsNullOrWhiteSpace(method) || AllowedPaymentMethods.Contains(method.Trim()))
            .WithMessage("PAYMENT_METHOD_INVALID|PaymentMethod must be one of: COD, VNPAY, MOMO, STRIPE");
    }
}
