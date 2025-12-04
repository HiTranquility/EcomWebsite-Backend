using App.BLL.Dtos.ProductDto.Shares;
using App.UTIL.Abstractions.DTO.Result;

namespace App.BLL.Dtos.ProductDto.Results;

public class ProductInformationRes : BaseResult
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public string? Currency { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? Rating { get; set; }
    public int? ReviewCount { get; set; }
    public string? ThumbnailUrl { get; set; }
    public List<ProductImageItem> ImageUrls { get; set; } = new();
    public List<ProductCategoryItem> Categories { get; set; } = new();
    public List<ProductCategoryItem> BreadcrumbPath { get; set; } = new(); // Path from root to current category
    public List<ProductTagItem> Tags { get; set; } = new();
    public uint? Quantity { get; set; }
    public uint? StockLeft { get; set; }
    public string? Size { get; set; }
    public string? ManufacturerName { get; set; }
    public int? ManufacturerId { get; set; }
    public int? ManufacturerTotal { get; set; }
    public string? CategoryName { get; set; }
    public bool? IsFreeShipping { get; set; }
    public bool? IsFlashsale { get; set; }
    public bool? IsFeature { get; set; }
    public bool? IsSpecial { get; set; }
    public bool? IsWeekly { get; set; }
    public bool? IsToday { get; set; }
    public bool? IsDeal { get; set; }
    public DateTime CreatedAt { get; set; }
}

