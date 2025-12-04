using App.UTIL.Abstractions.DTO.Result;
using App.BLL.Dtos.BlogDto.Shares;

namespace App.BLL.Dtos.BlogDto.Results;

public class BlogDetailRes : BaseResult
{
    public int Id { get; init; }
    public string? Title { get; init; }
    public string? Slug { get; init; }
    public string? Content { get; init; }
    public string? ImageUrl { get; init; }
    public string? Author { get; init; }
    public int? BlogCommentCount { get; init; }
    public DateTime? CreatedAt { get; init; }

    public QuoteItem? Quote { get; init; }
    public List<CategoryItem>? AllCategories { get; init; }
    public List<TagItem>? AllTags { get; init; }
    public List<VariantItem>? AllVariants { get; init; }
    public NeighborBlogItem? NextBlog { get; set; }
    public NeighborBlogItem? PrevBlog { get; set; }
    // Uses shared DTOs
}

