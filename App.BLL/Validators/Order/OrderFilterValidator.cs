using App.BLL.Dtos.OrderDto;
using FluentValidation;

namespace App.BLL.Validators.Order;

/// <summary>
/// Validator for OrderFilter query parameters
/// </summary>
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
        RuleFor(x => x.Status)
            .Must(status => string.IsNullOrWhiteSpace(status) || AllowedStatuses.Contains(status.Trim()))
            .WithMessage("ORDER_STATUS_INVALID|Status must be one of: pending, paid, shipping, cancelled, refunded");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("PAGE_INVALID|Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PAGE_SIZE_INVALID|PageSize must be between 1 and 100");
    }
}
