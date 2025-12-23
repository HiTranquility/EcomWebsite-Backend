using App.UTIL.Abstractions.DTO.Result;

namespace App.BLL.Dtos.PaymentDto.Results;

#region --- Stripe Response DTOs ---

/// <summary>
/// Stripe Payment Intent response
/// </summary>
public class StripePaymentIntentRes : BaseResult
{
    public string ClientSecret { get; set; } = string.Empty;
    public string PaymentIntentId { get; set; } = string.Empty;
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
}

#endregion

#region --- MoMo Response DTOs ---

/// <summary>
/// MoMo payment response
/// </summary>
public class MoMoPaymentRes : BaseResult
{
    public string PayUrl { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
}

#endregion

#region --- VNPay Response DTOs ---

/// <summary>
/// VNPay payment response
/// </summary>
public class VNPayPaymentRes : BaseResult
{
    public string PaymentUrl { get; set; } = string.Empty;
    public string TxnRef { get; set; } = string.Empty;
}

#endregion

#region --- Common Response DTOs ---

/// <summary>
/// Generic payment status response
/// </summary>
public class PaymentStatusRes : BaseResult
{
    public string Status { get; set; } = string.Empty; // pending, processing, succeeded, failed, cancelled
    public int OrderId { get; set; }
    public string? TransactionId { get; set; }
    public DateTime? PaidAt { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty; // stripe, momo, vnpay, cod
}

#endregion
