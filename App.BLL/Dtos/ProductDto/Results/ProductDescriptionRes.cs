using System.Text.Json.Nodes;
using App.UTIL.Abstractions.DTO.Result;

namespace App.BLL.Dtos.ProductDto.Results;

public class ProductDescriptionRes : BaseResult
{
    public string? LongDescription { get; set; }
    public JsonArray? Information { get; set; }
}