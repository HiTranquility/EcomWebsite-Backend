namespace App.BLL.Dtos.ProductDto.Shares;

public sealed class ProductTagItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public sealed class ProductCategoryItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}


public sealed class ProductImageItem
{
    public string Id { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}