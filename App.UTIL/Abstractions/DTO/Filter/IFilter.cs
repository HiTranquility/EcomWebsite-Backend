namespace App.UTIL.Abstractions.DTO.Filter;

public interface IFilter
{
    int Page { get; set; }
    int PageSize { get; set; }
    string? Keyword { get; set; }
    void Normalize();
}