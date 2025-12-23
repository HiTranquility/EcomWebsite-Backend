using App.BLL.Dtos.PaymentDto.Requests;
using App.UTIL.Abstractions.DTO.Response;

namespace App.BLL.Interfaces;

public interface IPaymentSvc
{
    Task<BaseResponse> CreatePaymentAsync(int userId, int orderId, CreatePaymentReq request, CancellationToken ct = default);
}

