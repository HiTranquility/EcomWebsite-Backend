using App.BLL.Dtos.PaymentDto.Requests;
using App.BLL.Dtos.PaymentDto.Results;
using App.BLL.Interfaces;
using App.DAL.OrderModels;
using App.DAL.Repositories;
using App.UTIL.Abstractions.BLL;
using App.UTIL.Abstractions.DTO.Response;
using App.UTIL.Settings;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;

namespace App.BLL.Services;

public class StripePaymentSvc : GenericSvc<TransactionRepo, Transaction>, IStripePaymentSvc
{
    private readonly StripeSettings _settings;
    private readonly OrderRepo _orderRepo;

    public StripePaymentSvc(
        TransactionRepo repo,
        OrderRepo orderRepo,
        IOptions<StripeSettings> settings,
        IMapper mapper) : base(repo, mapper)
    {
        _settings = settings.Value;
        _orderRepo = orderRepo;
        
        if (!string.IsNullOrWhiteSpace(_settings.SecretKey))
            StripeConfiguration.ApiKey = _settings.SecretKey;
    }

    public async Task<BaseResponse> CreatePaymentIntentAsync(int userId, int orderId, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
        {
            rsp.SetError("STRIPE_NOT_CONFIGURED", "Stripe is not configured", "Payment not available", 503);
            return rsp;
        }

        var order = await _orderRepo.All
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, ct);

        if (order == null)
        {
            rsp.SetError("ORDER_NOT_FOUND", "Order not found", "Order not found", 404);
            return rsp;
        }

        if (string.Equals(order.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
        {
            rsp.SetError("ORDER_ALREADY_PAID", "Order is already paid", "Order already paid", 400);
            return rsp;
        }

        try
        {
            var amountInCents = (long)(order.FinalPrice * 100);

            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInCents,
                Currency = _settings.Currency,
                Metadata = new Dictionary<string, string>
                {
                    { "orderId", orderId.ToString() },
                    { "userId", userId.ToString() }
                },
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options, cancellationToken: ct);

            var tx = new Transaction
            {
                OrderId = orderId,
                GatewayTransactionCode = paymentIntent.Id,
                Method = "STRIPE",
                Amount = order.FinalPrice,
                Status = "pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _repo.CreateAsync(tx, ct);

            rsp.SetData(new StripePaymentIntentRes
            {
                ClientSecret = paymentIntent.ClientSecret ?? string.Empty,
                PaymentIntentId = paymentIntent.Id,
                OrderId = orderId,
                Amount = order.FinalPrice
            }, "Payment intent created successfully", 201);
        }
        catch (StripeException ex)
        {
            rsp.SetError("STRIPE_ERROR", ex.Message, "Payment processing error", 500);
        }
        catch (Exception ex)
        {
            rsp.SetError("PAYMENT_ERROR", ex.Message, "Failed to create payment", 500);
        }

        return rsp;
    }

    public async Task<BaseResponse> ConfirmPaymentAsync(ConfirmStripePaymentReq request, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(request.PaymentIntentId, cancellationToken: ct);

            if (paymentIntent == null)
            {
                rsp.SetError("PAYMENT_NOT_FOUND", "Payment not found", "Payment not found", 404);
                return rsp;
            }

            if (!paymentIntent.Metadata.TryGetValue("orderId", out var orderIdStr) || 
                !int.TryParse(orderIdStr, out var orderId))
            {
                rsp.SetError("INVALID_PAYMENT", "Invalid payment metadata", "Invalid payment", 400);
                return rsp;
            }

            var transaction = await _repo.All
                .FirstOrDefaultAsync(t => t.GatewayTransactionCode == request.PaymentIntentId, ct);

            if (transaction != null)
            {
                transaction.Status = paymentIntent.Status;
                transaction.UpdatedAt = DateTime.UtcNow;
                await _repo.UpdateAsync(transaction, ct);
            }

            if (paymentIntent.Status == "succeeded")
            {
                var order = await _orderRepo.All.FirstOrDefaultAsync(o => o.Id == orderId, ct);
                if (order != null)
                {
                    order.PaymentStatus = "paid";
                    order.UpdatedAt = DateTime.UtcNow;
                    await _orderRepo.UpdateAsync(order, ct);
                }
            }

            rsp.SetData(new PaymentStatusRes
            {
                Status = paymentIntent.Status switch
                {
                    "succeeded" => "succeeded",
                    "processing" => "processing",
                    "requires_payment_method" => "failed",
                    "requires_confirmation" or "requires_action" => "pending",
                    "canceled" => "cancelled",
                    _ => "pending"
                },
                OrderId = orderId,
                TransactionId = request.PaymentIntentId,
                PaidAt = paymentIntent.Status == "succeeded" ? DateTime.UtcNow : null,
                Amount = (decimal)paymentIntent.Amount / 100,
                Method = "stripe"
            }, "Payment confirmed", 200);
        }
        catch (StripeException ex)
        {
            rsp.SetError("STRIPE_ERROR", ex.Message, "Payment confirmation error", 500);
        }
        catch (Exception ex)
        {
            rsp.SetError("PAYMENT_ERROR", ex.Message, "Failed to confirm payment", 500);
        }

        return rsp;
    }

    public async Task<BaseResponse> HandleWebhookAsync(string json, string signature, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        try
        {
            if (string.IsNullOrWhiteSpace(_settings.WebhookSecret))
            {
                rsp.SetError("WEBHOOK_NOT_CONFIGURED", "Webhook secret not configured", "Webhook error", 500);
                return rsp;
            }

            var stripeEvent = EventUtility.ConstructEvent(json, signature, _settings.WebhookSecret);

            // payment_intent.succeeded
            if (stripeEvent.Type == "payment_intent.succeeded" && stripeEvent.Data.Object is PaymentIntent pi)
            {
                if (pi.Metadata.TryGetValue("orderId", out var oid) && int.TryParse(oid, out var orderId))
                {
                    var order = await _orderRepo.All.FirstOrDefaultAsync(o => o.Id == orderId, ct);
                    if (order != null && order.PaymentStatus != "paid")
                    {
                        order.PaymentStatus = "paid";
                        order.UpdatedAt = DateTime.UtcNow;
                        await _orderRepo.UpdateAsync(order, ct);
                    }
                }
            }
            // payment_intent.payment_failed
            else if (stripeEvent.Type == "payment_intent.payment_failed" && stripeEvent.Data.Object is PaymentIntent failedPi)
            {
                var tx = await _repo.All.FirstOrDefaultAsync(t => t.GatewayTransactionCode == failedPi.Id, ct);
                if (tx != null)
                {
                    tx.Status = "failed";
                    tx.UpdatedAt = DateTime.UtcNow;
                    await _repo.UpdateAsync(tx, ct);
                }
            }

            rsp.SetMessage("Webhook processed", 200);
        }
        catch (StripeException ex)
        {
            rsp.SetError("WEBHOOK_ERROR", ex.Message, "Webhook error", 400);
        }

        return rsp;
    }

}
