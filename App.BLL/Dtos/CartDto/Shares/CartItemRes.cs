namespace App.BLL.Dtos.CartDto.Shares;

public class CartItemRes
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public int ProductId { get; set; }
    public int? VariantId { get; set; }
    public int Quantity { get; set; }
    public decimal PriceAtTime { get; set; }
    public decimal Subtotal { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

