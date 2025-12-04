using App.UTIL.Abstractions.DTO.Result;

namespace App.BLL.Dtos.BlogDto.Results;

public class BlogCommentRes : BaseResult
{
    public int Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<BlogCommentRes> Children { get; set; } = new();
}
