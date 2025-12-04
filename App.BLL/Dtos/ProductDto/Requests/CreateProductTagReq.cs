using App.UTIL.Abstractions.DTO.Request;

namespace App.BLL.Dtos.ProductDto.Requests;

public class CreateProductTagReq : BaseRequest
{
    public string Name { get; set; } = string.Empty;
}

