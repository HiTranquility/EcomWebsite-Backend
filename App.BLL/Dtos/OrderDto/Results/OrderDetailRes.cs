using App.BLL.Dtos.OrderDto.Shares;

namespace App.BLL.Dtos.OrderDto.Results;

public class OrderDetailRes
{
    public int Id { get; set; }
    public string? Status { get; set; }
    public string? PaymentStatus { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal? ShippingFee { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }
    public DateTime? CreatedAt { get; set; }

    public List<OrderItemRes> Items { get; set; } = new();
}


