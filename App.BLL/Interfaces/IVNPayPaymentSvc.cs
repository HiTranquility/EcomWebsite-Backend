using App.BLL.Dtos.PaymentDto.Requests;
using App.UTIL.Abstractions.DTO.Response;

namespace App.BLL.Interfaces;

public interface IVNPayPaymentSvc
{
    Task<BaseResponse> CreatePaymentAsync(int userId, CreateVNPayPaymentReq request, CancellationToken ct = default);
    Task<BaseResponse> VerifyPaymentAsync(VerifyVNPayPaymentReq request, CancellationToken ct = default);
}
