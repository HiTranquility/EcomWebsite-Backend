using App.BLL.Dtos.ProductDto.Shares;

namespace App.BLL.Dtos.ProductDto.Results;

public class ProductFilterRes
{
    public IReadOnlyList<ProductFilterManufacturerItem> Manufacturers { get; init; } = new List<ProductFilterManufacturerItem>();
    public IReadOnlyList<ProductFilterCategoryRes> Categories { get; init; } = new List<ProductFilterCategoryRes>();
    public IReadOnlyList<ProductFilterRatingItem> Ratings { get; init; } = new List<ProductFilterRatingItem>();
    public IReadOnlyList<ProductFilterSizeItem> Sizes { get; init; } = new List<ProductFilterSizeItem>();
}

