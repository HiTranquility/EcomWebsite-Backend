using App.BLL.Dtos.PaymentDto.Requests;
using FluentValidation;

namespace App.BLL.Validators.Payment;

/// <summary>
/// Validator for ConfirmStripePaymentReq
/// </summary>
public class ConfirmStripePaymentReqValidator : AbstractValidator<ConfirmStripePaymentReq>
{
    public ConfirmStripePaymentReqValidator()
    {
        RuleFor(x => x.PaymentIntentId)
            .NotEmpty()
            .WithMessage("PAYMENT_INTENT_ID_REQUIRED|PaymentIntentId is required")
            .Matches(@"^pi_[a-zA-Z0-9]+$")
            .WithMessage("PAYMENT_INTENT_ID_INVALID|PaymentIntentId format is invalid. Expected format: pi_xxx");
    }
}
