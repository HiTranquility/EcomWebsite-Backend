using App.UTIL.Abstractions.DTO.Request;

namespace App.BLL.Dtos.ProductDto.Requests;

public class CreateReviewReq : BaseRequest
{
    public string? Content { get; set; }

    public string? FullName { get; set; }
    
    public string? Email { get; set; }
    
    public uint? StarRating { get; set; }
}