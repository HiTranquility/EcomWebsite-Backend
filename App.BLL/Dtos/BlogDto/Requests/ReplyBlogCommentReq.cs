using App.UTIL.Abstractions.DTO.Request;

namespace App.BLL.Dtos.BlogDto.Requests;

public class ReplyBlogCommentReq : BaseRequest
{
    public string Content { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Email { get; set; }
}


