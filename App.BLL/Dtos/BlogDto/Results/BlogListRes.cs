using System;
using System.Collections.Generic;
using System.Linq;
using App.BLL.Dtos.BlogDto.Shares;
using App.UTIL.Abstractions.DTO.Result;

namespace App.BLL.Dtos.BlogDto.Results;

public class BlogListRes : BaseResult
{
    public int Id { get; init; }
    public string? Title { get; init; }
    public string? Slug { get; init; }
    public string? Content { get; init; }
    public string? Author { get; init; }
    public List<CategoryItem>? AllCategories { get; init; }
    public List<TagItem>? AllTags { get; init; }
    public List<VariantItem>? AllVariants { get; init; }
    public DateTime CreatedAt { get; init; } 
    public int? BlogCommentCount { get; init; }
    // Uses shared DTOs
}