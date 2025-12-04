namespace App.BLL.Dtos.BlogDto.Shares;

public sealed class VariantItem
{
    public string Type { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public List<string>? Urls { get; init; }
    public int? Id { get; init; }
}


