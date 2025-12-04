using App.BLL.Dtos.CartDto.Shares;

namespace App.BLL.Dtos.CartDto.Results;

public class CartRes
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal TotalPrice { get; set; }
    public int TotalQuantity { get; set; }
    public string? Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<CartItemRes> Items { get; set; } = new();
}

