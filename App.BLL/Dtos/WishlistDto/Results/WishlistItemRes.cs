namespace App.BLL.Dtos.WishlistDto.Results;

public class WishlistItemRes
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public decimal? Price { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DateTime? CreatedAt { get; set; }
}


