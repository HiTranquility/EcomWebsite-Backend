using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using App.UTIL.Abstractions.DTO.Filter;

namespace App.BLL.Dtos.BlogDto;

public class BlogFilter : BaseFilter
{
    protected override int MinPageSize => 6;
    protected override int MaxPageSize => 36;
    protected override int DefaultPageSize => 10;

    [StringLength(50, ErrorMessage = "BLOG_CATEGORY_LENGTH_INVALID|Category length must not exceed 50 characters")]
    [FromQuery(Name = "category")]
    public string? Category { get; set; } = string.Empty;

    [MaxLength(20, ErrorMessage = "BLOG_TAGS_TOO_MANY|Tags must not exceed 20 items.")]
    [FromQuery(Name = "tags")]
    public string[]? Tags { get; set; }

    [RegularExpression("^(newest|oldest)$", ErrorMessage = "BLOG_SORT_INVALID|Sort must be 'newest' or 'oldest'.")]
    [FromQuery(Name = "sort")]
    public string? Sort { get; set; }

    [StringLength(50, ErrorMessage = "BLOG_AUTHOR_LENGTH_INVALID|Author length must not exceed 50 characters")]
    [FromQuery(Name = "author")]
    public string? Author { get; set; }
}