namespace App.BLL.Dtos.OrderDto.Shares;

public class OrderItemRes
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int? VariantId { get; set; }
    public int Quantity { get; set; }
    public decimal PriceAtTime { get; set; }
}


