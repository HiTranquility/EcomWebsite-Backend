using App.UTIL.Abstractions.DTO.Request;

namespace App.BLL.Dtos.OrderDto.Requests;

public class CreateOrderReq : BaseRequest
{
    public int CartId { get; set; }

    public string? DeliveryType { get; set; } = "standard";

    public decimal? ShippingFee { get; set; }

    public decimal? DiscountAmount { get; set; }

    public string? PaymentMethod { get; set; }
}


