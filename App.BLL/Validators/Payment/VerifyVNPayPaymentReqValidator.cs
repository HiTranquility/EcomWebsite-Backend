using App.BLL.Dtos.PaymentDto.Requests;
using FluentValidation;

namespace App.BLL.Validators.Payment;

/// <summary>
/// Validator for VerifyVNPayPaymentReq (return URL callback)
/// </summary>
public class VerifyVNPayPaymentReqValidator : AbstractValidator<VerifyVNPayPaymentReq>
{
    public VerifyVNPayPaymentReqValidator()
    {
        RuleFor(x => x.vnp_TmnCode)
            .NotEmpty()
            .WithMessage("TMN_CODE_REQUIRED|vnp_TmnCode is required");

        RuleFor(x => x.vnp_Amount)
            .NotEmpty()
            .WithMessage("AMOUNT_REQUIRED|vnp_Amount is required");

        RuleFor(x => x.vnp_TxnRef)
            .NotEmpty()
            .WithMessage("TXN_REF_REQUIRED|vnp_TxnRef is required");

        RuleFor(x => x.vnp_ResponseCode)
            .NotEmpty()
            .WithMessage("RESPONSE_CODE_REQUIRED|vnp_ResponseCode is required");

        RuleFor(x => x.vnp_SecureHash)
            .NotEmpty()
            .WithMessage("SECURE_HASH_REQUIRED|vnp_SecureHash is required for verification");
    }
}
