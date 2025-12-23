using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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

namespace App.BLL.Services;

public class MoMoPaymentSvc : GenericSvc<TransactionRepo, Transaction>, IMoMoPaymentSvc
{
    private readonly MoMoSettings _settings;
    private readonly OrderRepo _orderRepo;
    private readonly IHttpClientFactory _httpClientFactory;

    public MoMoPaymentSvc(
        TransactionRepo repo,
        OrderRepo orderRepo,
        IOptions<MoMoSettings> settings,
        IHttpClientFactory httpClientFactory,
        IMapper mapper) : base(repo, mapper)
    {
        _settings = settings.Value;
        _orderRepo = orderRepo;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<BaseResponse> CreatePaymentAsync(int userId, CreateMoMoPaymentReq request, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        if (string.IsNullOrWhiteSpace(_settings.PartnerCode) || string.IsNullOrWhiteSpace(_settings.SecretKey))
        {
            rsp.SetError("MOMO_NOT_CONFIGURED", "MoMo is not configured", "Payment not available", 503);
            return rsp;
        }

        var order = await _orderRepo.All
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == userId, ct);

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
            var momoRequestId = Guid.NewGuid().ToString();
            var momoOrderId = $"ORDER{request.OrderId}_{DateTime.UtcNow:yyyyMMddHHmmss}";
            var amount = (long)order.FinalPrice;
            var orderInfo = $"Payment for Order #{request.OrderId}";
            var extraData = Convert.ToBase64String(Encoding.UTF8.GetBytes($"orderId={request.OrderId}"));

            var rawSignature = $"accessKey={_settings.AccessKey}&amount={amount}&extraData={extraData}" +
                              $"&ipnUrl={_settings.IpnUrl}&orderId={momoOrderId}&orderInfo={orderInfo}" +
                              $"&partnerCode={_settings.PartnerCode}&redirectUrl={request.ReturnUrl}" +
                              $"&requestId={momoRequestId}&requestType=payWithMethod";

            var signature = ComputeHmacSha256(rawSignature, _settings.SecretKey);

            var requestBody = new
            {
                partnerCode = _settings.PartnerCode,
                partnerName = "EcomWebsite",
                storeId = _settings.PartnerCode,
                requestId = momoRequestId,
                amount,
                orderId = momoOrderId,
                orderInfo,
                redirectUrl = request.ReturnUrl,
                ipnUrl = _settings.IpnUrl,
                lang = "vi",
                requestType = "payWithMethod",
                autoCapture = true,
                extraData,
                signature
            };

            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{_settings.Endpoint}/create", content, ct);
            var responseContent = await response.Content.ReadAsStringAsync(ct);

            var momoResponse = JsonSerializer.Deserialize<MoMoCreateResponse>(responseContent, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (momoResponse == null || momoResponse.ResultCode != 0)
            {
                rsp.SetError("MOMO_ERROR", momoResponse?.Message ?? "MoMo error", "Payment processing error", 400);
                return rsp;
            }

            var tx = new Transaction
            {
                OrderId = request.OrderId,
                GatewayTransactionCode = momoOrderId,
                Method = "MOMO",
                Amount = order.FinalPrice,
                Status = "pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _repo.CreateAsync(tx, ct);

            rsp.SetData(new MoMoPaymentRes
            {
                PayUrl = momoResponse.PayUrl ?? string.Empty,
                OrderId = momoOrderId,
                RequestId = momoRequestId
            }, "MoMo payment created", 201);
        }
        catch (Exception ex)
        {
            rsp.SetError("PAYMENT_ERROR", ex.Message, "Failed to create payment", 500);
        }

        return rsp;
    }

    public async Task<BaseResponse> VerifyPaymentAsync(VerifyMoMoPaymentReq request, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        try
        {
            var rawSignature = $"accessKey={_settings.AccessKey}&amount={request.Amount}&extraData={request.ExtraData}" +
                              $"&message={request.Message}&orderId={request.OrderId}&orderInfo={request.OrderInfo}" +
                              $"&orderType={request.OrderType}&partnerCode={request.PartnerCode}&payType={request.PayType}" +
                              $"&requestId={request.RequestId}&responseTime={request.ResponseTime}" +
                              $"&resultCode={request.ResultCode}&transId={request.TransId}";

            var expectedSignature = ComputeHmacSha256(rawSignature, _settings.SecretKey);

            if (!string.Equals(expectedSignature, request.Signature, StringComparison.OrdinalIgnoreCase))
            {
                rsp.SetError("INVALID_SIGNATURE", "Invalid signature", "Payment verification failed", 400);
                return rsp;
            }

            var extraDataBytes = Convert.FromBase64String(request.ExtraData);
            var extraDataStr = Encoding.UTF8.GetString(extraDataBytes);
            var orderIdMatch = System.Text.RegularExpressions.Regex.Match(extraDataStr, @"orderId=(\d+)");
            
            if (!orderIdMatch.Success || !int.TryParse(orderIdMatch.Groups[1].Value, out var orderId))
            {
                rsp.SetError("INVALID_ORDER", "Invalid order ID", "Invalid order", 400);
                return rsp;
            }

            var transaction = await _repo.All.FirstOrDefaultAsync(t => t.GatewayTransactionCode == request.OrderId, ct);
            var status = request.ResultCode == 0 ? "succeeded" : "failed";

            if (transaction != null)
            {
                transaction.Status = status;
                transaction.UpdatedAt = DateTime.UtcNow;
                await _repo.UpdateAsync(transaction, ct);
            }

            if (request.ResultCode == 0)
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
                Status = status,
                OrderId = orderId,
                TransactionId = request.TransId.ToString(),
                PaidAt = request.ResultCode == 0 ? DateTime.UtcNow : null,
                Amount = request.Amount,
                Method = "momo"
            }, "Payment verified", 200);
        }
        catch (Exception ex)
        {
            rsp.SetError("PAYMENT_ERROR", ex.Message, "Failed to verify payment", 500);
        }

        return rsp;
    }

    private static string ComputeHmacSha256(string data, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}

internal class MoMoCreateResponse
{
    public string? PartnerCode { get; set; }
    public string? OrderId { get; set; }
    public string? RequestId { get; set; }
    public long Amount { get; set; }
    public long ResponseTime { get; set; }
    public string? Message { get; set; }
    public int ResultCode { get; set; }
    public string? PayUrl { get; set; }
    public string? DeepLink { get; set; }
    public string? QrCodeUrl { get; set; }
}
