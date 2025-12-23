using System;
using System.Collections.Generic;

namespace App.DAL.OrderModels;

public partial class Transaction
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public decimal Amount { get; set; }

    public string Method { get; set; } = null!;

    public string? GatewayTransactionCode { get; set; }

    public string? Status { get; set; }

    /// <summary>
    /// Transaction ID from payment provider (Stripe PaymentIntent ID, MoMo TransId, VNPay TransactionNo)
    /// </summary>
    public string? ProviderTransactionId { get; set; }

    /// <summary>
    /// Refund ID from payment provider (if refunded)
    /// </summary>
    public string? ProviderRefundId { get; set; }

    /// <summary>
    /// Currency code (VND, USD, etc.)
    /// </summary>
    public string Currency { get; set; } = "VND";

    /// <summary>
    /// Reason for payment failure (if failed)
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// JSON containing payer information (email, phone, card last 4 digits, etc.)
    /// </summary>
    public string? PayerInfo { get; set; }

    /// <summary>
    /// Amount refunded (for partial refunds)
    /// </summary>
    public decimal? RefundAmount { get; set; }

    /// <summary>
    /// When the refund was processed
    /// </summary>
    public DateTime? RefundedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual ICollection<PaymentsLog> PaymentsLogs { get; set; } = new List<PaymentsLog>();
}
