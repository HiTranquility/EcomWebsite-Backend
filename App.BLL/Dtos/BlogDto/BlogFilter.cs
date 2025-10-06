using System.ComponentModel.DataAnnotations;
using App.UTIL.Abstractions.DTO.Filter;

namespace App.BLL.Dtos.BlogDto;

public class BlogFilter : BaseFilter
{
    [StringLength(50, ErrorMessage = "BLOG_CATEGORY_LENGTH_INVALID|Category length must not exceed 50 characters")]
    public string? BlogCategory { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "BLOG_TAG_LENGTH_INVALID|Tag length must not exceed 50 characters")]
    public string? BlogTag { get; set; } = string.Empty;
}