using App.BLL.Dtos.PaymentDto.Requests;
using FluentValidation;

namespace App.BLL.Validators.Payment;

/// <summary>
/// Validator for VerifyMoMoPaymentReq (IPN callback)
/// </summary>
public class VerifyMoMoPaymentReqValidator : AbstractValidator<VerifyMoMoPaymentReq>
{
    public VerifyMoMoPaymentReqValidator()
    {
        RuleFor(x => x.PartnerCode)
            .NotEmpty()
            .WithMessage("PARTNER_CODE_REQUIRED|PartnerCode is required");

        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("ORDER_ID_REQUIRED|OrderId is required");

        RuleFor(x => x.RequestId)
            .NotEmpty()
            .WithMessage("REQUEST_ID_REQUIRED|RequestId is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("AMOUNT_INVALID|Amount must be greater than 0");

        RuleFor(x => x.Signature)
            .NotEmpty()
            .WithMessage("SIGNATURE_REQUIRED|Signature is required for verification");
    }
}
