using App.BLL.Dtos.PaymentDto.Requests;
using App.UTIL.Abstractions.DTO.Response;

namespace App.BLL.Interfaces;

public interface IMoMoPaymentSvc
{
    Task<BaseResponse> CreatePaymentAsync(int userId, CreateMoMoPaymentReq request, CancellationToken ct = default);
    Task<BaseResponse> VerifyPaymentAsync(VerifyMoMoPaymentReq request, CancellationToken ct = default);
}
