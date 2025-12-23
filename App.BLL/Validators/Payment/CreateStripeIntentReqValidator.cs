using App.BLL.Dtos.PaymentDto.Requests;
using FluentValidation;

namespace App.BLL.Validators.Payment;

/// <summary>
/// Validator for CreateStripeIntentReq
/// </summary>
public class CreateStripeIntentReqValidator : AbstractValidator<CreateStripeIntentReq>
{
    public CreateStripeIntentReqValidator()
    {
        RuleFor(x => x.OrderId)
            .GreaterThan(0)
            .WithMessage("ORDER_ID_INVALID|OrderId must be a positive integer");
    }
}
