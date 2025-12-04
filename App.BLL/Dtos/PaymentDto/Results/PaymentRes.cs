using App.UTIL.Abstractions.DTO.Result;

namespace App.BLL.Dtos.PaymentDto.Results;

public class PaymentRes : BaseResult
{
    public int TransactionId { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string? Status { get; set; }
    public string? GatewayTransactionCode { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

