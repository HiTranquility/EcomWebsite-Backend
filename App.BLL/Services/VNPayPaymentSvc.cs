using System.Net;
using System.Security.Cryptography;
using System.Text;
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

public class VNPayPaymentSvc : GenericSvc<TransactionRepo, Transaction>, IVNPayPaymentSvc
{
    private readonly VNPaySettings _settings;
    private readonly OrderRepo _orderRepo;

    public VNPayPaymentSvc(
        TransactionRepo repo,
        OrderRepo orderRepo,
        IOptions<VNPaySettings> settings,
        IMapper mapper) : base(repo, mapper)
    {
        _settings = settings.Value;
        _orderRepo = orderRepo;
    }

    public async Task<BaseResponse> CreatePaymentAsync(int userId, CreateVNPayPaymentReq request, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        if (string.IsNullOrWhiteSpace(_settings.TmnCode) || string.IsNullOrWhiteSpace(_settings.HashSecret))
        {
            rsp.SetError("VNPAY_NOT_CONFIGURED", "VNPay is not configured", "Payment not available", 503);
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
            var txnRef = $"{request.OrderId}_{DateTime.UtcNow:yyyyMMddHHmmss}";
            var createDate = DateTime.Now.ToString("yyyyMMddHHmmss");
            var expireDate = DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss");
            var amount = (long)(order.FinalPrice * 100);
            var orderInfo = $"Payment for Order {request.OrderId}";

            var vnpParams = new SortedDictionary<string, string>
            {
                { "vnp_Version", _settings.Version },
                { "vnp_Command", _settings.Command },
                { "vnp_TmnCode", _settings.TmnCode },
                { "vnp_Amount", amount.ToString() },
                { "vnp_CurrCode", _settings.CurrCode },
                { "vnp_TxnRef", txnRef },
                { "vnp_OrderInfo", orderInfo },
                { "vnp_OrderType", "other" },
                { "vnp_Locale", _settings.Locale },
                { "vnp_ReturnUrl", request.ReturnUrl },
                { "vnp_IpAddr", "127.0.0.1" },
                { "vnp_CreateDate", createDate },
                { "vnp_ExpireDate", expireDate }
            };

            if (!string.IsNullOrEmpty(request.BankCode))
                vnpParams.Add("vnp_BankCode", request.BankCode);

            var queryString = BuildQueryString(vnpParams);
            var secureHash = ComputeHmacSha512(queryString, _settings.HashSecret);
            var paymentUrl = $"{_settings.PaymentUrl}?{queryString}&vnp_SecureHash={secureHash}";

            var tx = new Transaction
            {
                OrderId = request.OrderId,
                GatewayTransactionCode = txnRef,
                Method = "VNPAY",
                Amount = order.FinalPrice,
                Status = "pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _repo.CreateAsync(tx, ct);

            rsp.SetData(new VNPayPaymentRes
            {
                PaymentUrl = paymentUrl,
                TxnRef = txnRef
            }, "VNPay payment URL created", 201);
        }
        catch (Exception ex)
        {
            rsp.SetError("PAYMENT_ERROR", ex.Message, "Failed to create payment", 500);
        }

        return rsp;
    }

    public async Task<BaseResponse> VerifyPaymentAsync(VerifyVNPayPaymentReq request, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        try
        {
            var vnpParams = new SortedDictionary<string, string>
            {
                { "vnp_TmnCode", request.vnp_TmnCode },
                { "vnp_Amount", request.vnp_Amount },
                { "vnp_BankCode", request.vnp_BankCode },
                { "vnp_BankTranNo", request.vnp_BankTranNo },
                { "vnp_CardType", request.vnp_CardType },
                { "vnp_PayDate", request.vnp_PayDate },
                { "vnp_OrderInfo", request.vnp_OrderInfo },
                { "vnp_TransactionNo", request.vnp_TransactionNo },
                { "vnp_ResponseCode", request.vnp_ResponseCode },
                { "vnp_TransactionStatus", request.vnp_TransactionStatus },
                { "vnp_TxnRef", request.vnp_TxnRef }
            };

            var filteredParams = vnpParams.Where(kv => !string.IsNullOrEmpty(kv.Value))
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            var queryString = BuildQueryString(new SortedDictionary<string, string>(filteredParams));
            var expectedHash = ComputeHmacSha512(queryString, _settings.HashSecret);

            if (!string.Equals(expectedHash, request.vnp_SecureHash, StringComparison.OrdinalIgnoreCase))
            {
                rsp.SetError("INVALID_SIGNATURE", "Invalid signature", "Payment verification failed", 400);
                return rsp;
            }

            var txnRefParts = request.vnp_TxnRef.Split('_');
            if (!int.TryParse(txnRefParts[0], out var orderId))
            {
                rsp.SetError("INVALID_ORDER", "Invalid order ID", "Invalid order", 400);
                return rsp;
            }

            var isSuccess = request.vnp_ResponseCode == "00" && request.vnp_TransactionStatus == "00";
            var status = isSuccess ? "succeeded" : "failed";

            var transaction = await _repo.All.FirstOrDefaultAsync(t => t.GatewayTransactionCode == request.vnp_TxnRef, ct);
            if (transaction != null)
            {
                transaction.Status = status;
                transaction.UpdatedAt = DateTime.UtcNow;
                await _repo.UpdateAsync(transaction, ct);
            }

            if (isSuccess)
            {
                var order = await _orderRepo.All.FirstOrDefaultAsync(o => o.Id == orderId, ct);
                if (order != null)
                {
                    order.PaymentStatus = "paid";
                    order.UpdatedAt = DateTime.UtcNow;
                    await _orderRepo.UpdateAsync(order, ct);
                }
            }

            long.TryParse(request.vnp_Amount, out var amountRaw);

            rsp.SetData(new PaymentStatusRes
            {
                Status = status,
                OrderId = orderId,
                TransactionId = request.vnp_TransactionNo,
                PaidAt = isSuccess ? DateTime.UtcNow : null,
                Amount = (decimal)amountRaw / 100,
                Method = "vnpay"
            }, "Payment verified", 200);
        }
        catch (Exception ex)
        {
            rsp.SetError("PAYMENT_ERROR", ex.Message, "Failed to verify payment", 500);
        }

        return rsp;
    }

    private static string BuildQueryString(SortedDictionary<string, string> parameters)
    {
        var queryParts = parameters.Where(kv => !string.IsNullOrEmpty(kv.Value))
            .Select(kv => $"{kv.Key}={WebUtility.UrlEncode(kv.Value)}");
        return string.Join("&", queryParts);
    }

    private static string ComputeHmacSha512(string data, string key)
    {
        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}
