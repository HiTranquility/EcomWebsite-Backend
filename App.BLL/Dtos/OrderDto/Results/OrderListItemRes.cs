namespace App.BLL.Dtos.OrderDto.Results;

public class OrderListItemRes
{
    public int Id { get; set; }
    public string? Status { get; set; }
    public string? PaymentStatus { get; set; }
    public decimal FinalPrice { get; set; }
    public DateTime? CreatedAt { get; set; }
}


