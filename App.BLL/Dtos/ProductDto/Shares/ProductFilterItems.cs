namespace App.BLL.Dtos.ProductDto.Shares;

public sealed class ProductFilterCategoryRes
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Total { get; set; }
    public List<ProductFilterCategoryRes> Children { get; set; } = new();
}

public sealed class ProductFilterRatingItem
{
    public int Star { get; set; }
    public int Total { get; set; }
}

public sealed class ProductFilterSizeItem
{
    public string Label { get; set; } = string.Empty;
    public int Total { get; set; }
}

public sealed class ProductFilterManufacturerItem
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Total { get; set; }
}