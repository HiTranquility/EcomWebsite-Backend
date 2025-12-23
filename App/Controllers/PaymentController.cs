using App.BLL.Dtos.PaymentDto.Requests;
using App.BLL.Interfaces;
using App.UTIL.Extensions;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

/// <summary>
/// Payment Controller - Handles Stripe, MoMo, VNPay payment integrations
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/payments")]
public class PaymentController : ControllerBase
{
    private readonly IStripePaymentSvc _stripePaymentSvc;
    private readonly IMoMoPaymentSvc _momoPaymentSvc;
    private readonly IVNPayPaymentSvc _vnpayPaymentSvc;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IStripePaymentSvc stripePaymentSvc,
        IMoMoPaymentSvc momoPaymentSvc,
        IVNPayPaymentSvc vnpayPaymentSvc,
        ILogger<PaymentController> logger)
    {
        _stripePaymentSvc = stripePaymentSvc;
        _momoPaymentSvc = momoPaymentSvc;
        _vnpayPaymentSvc = vnpayPaymentSvc;
        _logger = logger;
    }

    #region Stripe

    /// <summary>
    /// Create Stripe Payment Intent
    /// </summary>
    [Authorize]
    [HttpPost("stripe/create-intent")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateStripeIntent([FromBody] CreateStripeIntentReq request, CancellationToken ct)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        var result = await _stripePaymentSvc.CreatePaymentIntentAsync(userId.Value, request.OrderId);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Confirm Stripe Payment
    /// </summary>
    [Authorize]
    [HttpPost("stripe/confirm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmStripePayment([FromBody] ConfirmStripePaymentReq request)
    {
        var result = await _stripePaymentSvc.ConfirmPaymentAsync(request);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Stripe Webhook Handler
    /// </summary>
    [HttpPost("stripe/webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"];

        if (string.IsNullOrEmpty(signature))
        {
            return BadRequest("Missing Stripe signature");
        }

        var result = await _stripePaymentSvc.HandleWebhookAsync(json, signature!);
        return StatusCode(result.Status, result);
    }

    #endregion

    #region MoMo

    /// <summary>
    /// Create MoMo Payment
    /// </summary>
    [Authorize]
    [HttpPost("momo/create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateMoMoPayment([FromBody] CreateMoMoPaymentReq request, CancellationToken ct)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        var result = await _momoPaymentSvc.CreatePaymentAsync(userId.Value, request);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Verify MoMo Payment (IPN callback)
    /// </summary>
    [HttpPost("momo/verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyMoMoPayment([FromBody] VerifyMoMoPaymentReq request)
    {
        var result = await _momoPaymentSvc.VerifyPaymentAsync(request);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// MoMo IPN Handler (for MoMo server-to-server notification)
    /// </summary>
    [HttpPost("momo/ipn")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MoMoIpn([FromBody] VerifyMoMoPaymentReq request)
    {
        _logger.LogInformation("MoMo IPN received for order {OrderId}", request.OrderId);
        
        var result = await _momoPaymentSvc.VerifyPaymentAsync(request);
        
        // MoMo expects specific response format
        if (result.Success)
        {
            return Ok(new { resultCode = 0, message = "Received" });
        }
        
        return Ok(new { resultCode = 1, message = result.Message });
    }

    #endregion

    #region VNPay

    /// <summary>
    /// Create VNPay Payment URL
    /// </summary>
    [Authorize]
    [HttpPost("vnpay/create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateVNPayPayment([FromBody] CreateVNPayPaymentReq request, CancellationToken ct)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue) return Unauthorized();

        var result = await _vnpayPaymentSvc.CreatePaymentAsync(userId.Value, request);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Verify VNPay Payment (return URL handler)
    /// </summary>
    [HttpPost("vnpay/verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyVNPayPayment([FromBody] VerifyVNPayPaymentReq request)
    {
        var result = await _vnpayPaymentSvc.VerifyPaymentAsync(request);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// VNPay IPN Handler (for VNPay server-to-server notification)
    /// </summary>
    [HttpGet("vnpay/ipn")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> VNPayIpn([FromQuery] VerifyVNPayPaymentReq query)
    {
        _logger.LogInformation("VNPay IPN received for txn {TxnRef}", query.vnp_TxnRef);
        
        var result = await _vnpayPaymentSvc.VerifyPaymentAsync(query);
        
        // VNPay expects specific response format
        if (result.Success)
        {
            return Ok(new { RspCode = "00", Message = "Confirm Success" });
        }
        
        return Ok(new { RspCode = "99", Message = result.Message });
    }

    #endregion

    #region Common

    /// <summary>
    /// Get payment status for an order
    /// </summary>
    [Authorize]
    [HttpGet("status/{orderId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public Task<IActionResult> GetPaymentStatus(int orderId)
    {
        // TODO: Implement payment status check from database
        return Task.FromResult<IActionResult>(Ok(new { status = "pending", orderId }));
    }

    /// <summary>
    /// Cancel pending payment
    /// </summary>
    [Authorize]
    [HttpPost("cancel/{orderId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public Task<IActionResult> CancelPayment(int orderId)
    {
        // TODO: Implement payment cancellation
        return Task.FromResult<IActionResult>(Ok(new { message = "Payment cancelled", orderId }));
    }

    #endregion
}
