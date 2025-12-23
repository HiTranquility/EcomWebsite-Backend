using App.BLL.Dtos.PaymentDto.Requests;
using FluentValidation;

namespace App.BLL.Validators.Payment;

/// <summary>
/// Validator for CreatePaymentReq
/// </summary>
public class CreatePaymentReqValidator : AbstractValidator<CreatePaymentReq>
{
    private static readonly HashSet<string> AllowedMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "COD",
        "VNPAY",
        "MOMO",
        "STRIPE"
    };

    public CreatePaymentReqValidator()
    {
        RuleFor(x => x.Method)
            .NotEmpty()
            .WithMessage("PAYMENT_METHOD_REQUIRED|Payment method is required")
            .Must(method => AllowedMethods.Contains(method.Trim()))
            .WithMessage("PAYMENT_METHOD_INVALID|Payment method must be one of: COD, VNPAY, MOMO, STRIPE");
    }
}
