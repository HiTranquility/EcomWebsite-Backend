using App.UTIL.Abstractions.DTO.Request;

namespace App.BLL.Dtos.PaymentDto.Requests;

public class CreatePaymentReq :BaseRequest
{
    public string Method { get; set; } = "COD";
}

