using App.BLL.Dtos.OrderDto;
using FluentValidation;

namespace App.BLL.Validators;

public class OrderFilterValidator : AbstractValidator<OrderFilter>
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "pending",
        "paid",
        "shipping",
        "cancelled",
        "refunded"
    };

    public OrderFilterValidator()
    {
        RuleFor(f => f.Status)
            .Must(status => string.IsNullOrWhiteSpace(status) || AllowedStatuses.Contains(status.Trim()))
            .WithMessage("ORDER_STATUS_INVALID|Status must be one of pending, paid, shipping, cancelled, refunded");
    }
}


