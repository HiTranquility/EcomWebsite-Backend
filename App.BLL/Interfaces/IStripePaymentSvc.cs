using App.BLL.Dtos.PaymentDto.Requests;
using App.UTIL.Abstractions.DTO.Response;

namespace App.BLL.Interfaces;

public interface IStripePaymentSvc
{
    Task<BaseResponse> CreatePaymentIntentAsync(int userId, int orderId, CancellationToken ct = default);
    Task<BaseResponse> ConfirmPaymentAsync(ConfirmStripePaymentReq request, CancellationToken ct = default);
    Task<BaseResponse> HandleWebhookAsync(string json, string signature, CancellationToken ct = default);
}
