using System.ComponentModel.DataAnnotations;

namespace App.UTIL.Abstractions.DTO.Filter;

public class BaseFilter : IFilter    
{
    [Range(1, int.MaxValue, ErrorMessage = "PAGE_INVALID|Page must be greater than 0.")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PAGESIZE_INVALID|PageSize must be between 1 and 100.")]
    public int PageSize { get; set; } = 10;

    [MaxLength(100, ErrorMessage = "KEYWORD_TOO_LONG|Keyword must not exceed 100 characters.")]
    public string? Keyword { get; set; }
}