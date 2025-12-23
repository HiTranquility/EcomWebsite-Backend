using App.BLL.Dtos.PaymentDto.Requests;
using FluentValidation;

namespace App.BLL.Validators.Payment;

/// <summary>
/// Validator for CreateVNPayPaymentReq
/// </summary>
public class CreateVNPayPaymentReqValidator : AbstractValidator<CreateVNPayPaymentReq>
{
    public CreateVNPayPaymentReqValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0)
            .WithMessage("ORDER_ID_INVALID|OrderId must be a positive integer");

        RuleFor(x => x.ReturnUrl)
            .NotEmpty()
            .WithMessage("RETURN_URL_REQUIRED|ReturnUrl is required for VNPay payment")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) && 
                         (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .When(x => !string.IsNullOrEmpty(x.ReturnUrl))
            .WithMessage("RETURN_URL_INVALID|ReturnUrl must be a valid HTTP/HTTPS URL");

        RuleFor(x => x.BankCode)
            .MaximumLength(20)
            .When(x => !string.IsNullOrEmpty(x.BankCode))
            .WithMessage("BANK_CODE_INVALID|BankCode must not exceed 20 characters");
    }
}
