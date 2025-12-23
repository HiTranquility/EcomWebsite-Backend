using App.BLL.Dtos.PaymentDto.Requests;
using FluentValidation;

namespace App.BLL.Validators.Payment;

/// <summary>
/// Validator for CreateMoMoPaymentReq
/// </summary>
public class CreateMoMoPaymentReqValidator : AbstractValidator<CreateMoMoPaymentReq>
{
    public CreateMoMoPaymentReqValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0)
            .WithMessage("ORDER_ID_INVALID|OrderId must be a positive integer");

        RuleFor(x => x.ReturnUrl)
            .NotEmpty()
            .WithMessage("RETURN_URL_REQUIRED|ReturnUrl is required for MoMo payment")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) && 
                         (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .When(x => !string.IsNullOrEmpty(x.ReturnUrl))
            .WithMessage("RETURN_URL_INVALID|ReturnUrl must be a valid HTTP/HTTPS URL");
    }
}
