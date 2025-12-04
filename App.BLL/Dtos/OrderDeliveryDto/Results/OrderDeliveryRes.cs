namespace App.BLL.Dtos.OrderDeliveryDto.Results;

public class OrderDeliveryRes
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string? DeliveryType { get; set; }
    public string? DeliveryStatus { get; set; }
    public string? ShippingProvider { get; set; }
    public string? TrackingCode { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

