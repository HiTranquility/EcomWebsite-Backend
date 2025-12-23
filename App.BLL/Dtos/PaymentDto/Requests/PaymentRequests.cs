using App.UTIL.Abstractions.DTO.Request;

namespace App.BLL.Dtos.PaymentDto.Requests;

#region --- Stripe Request DTOs ---

/// <summary>
/// Request to create Stripe payment intent
/// </summary>
public class CreateStripeIntentReq : BaseRequest
{
    public int OrderId { get; set; }
}

/// <summary>
/// Request to confirm Stripe payment
/// </summary>
public class ConfirmStripePaymentReq : BaseRequest
{
    public string PaymentIntentId { get; set; } = string.Empty;
}

#endregion

#region --- MoMo Request DTOs ---

/// <summary>
/// Request to create MoMo payment
/// </summary>
public class CreateMoMoPaymentReq : BaseRequest
{
    public int OrderId { get; set; }
    public string ReturnUrl { get; set; } = string.Empty;
}

/// <summary>
/// MoMo IPN verification request - fields match MoMo API callback
/// </summary>
public class VerifyMoMoPaymentReq : BaseRequest
{
    public string PartnerCode { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string OrderInfo { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public long TransId { get; set; }
    public int ResultCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string PayType { get; set; } = string.Empty;
    public long ResponseTime { get; set; }
    public string ExtraData { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
}

#endregion

#region --- VNPay Request DTOs ---

/// <summary>
/// Request to create VNPay payment
/// </summary>
public class CreateVNPayPaymentReq : BaseRequest
{
    public int OrderId { get; set; }
    public string ReturnUrl { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty; // Optional: specific bank
}

/// <summary>
/// VNPay return URL verification request - fields match VNPay callback
/// </summary>
public class VerifyVNPayPaymentReq : BaseRequest
{
    public string vnp_TmnCode { get; set; } = string.Empty;
    public string vnp_Amount { get; set; } = string.Empty;
    public string vnp_BankCode { get; set; } = string.Empty;
    public string vnp_BankTranNo { get; set; } = string.Empty;
    public string vnp_CardType { get; set; } = string.Empty;
    public string vnp_PayDate { get; set; } = string.Empty;
    public string vnp_OrderInfo { get; set; } = string.Empty;
    public string vnp_TransactionNo { get; set; } = string.Empty;
    public string vnp_ResponseCode { get; set; } = string.Empty;
    public string vnp_TransactionStatus { get; set; } = string.Empty;
    public string vnp_TxnRef { get; set; } = string.Empty;
    public string vnp_SecureHash { get; set; } = string.Empty;
}

#endregion
