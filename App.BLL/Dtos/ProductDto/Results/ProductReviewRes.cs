using App.UTIL.Abstractions.DTO.Result;

namespace App.BLL.Dtos.ProductDto.Results;

public class ProductReviewRes : BaseResult
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public uint? StarRating { get; set; }
    public string? Content { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public int? UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

