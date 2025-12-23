using App.BLL.Dtos.BlogDto.Requests;
using App.UTIL.Abstractions.DTO.Response;

namespace App.BLL.Interfaces;

public interface IBlogCommentSvc
{
    Task<BaseResponse> CreateBlogCommentAsync(int blogId, int userId, CreateBlogCommentReq request, CancellationToken ct = default);
    Task<BaseResponse> ReplyBlogCommentAsync(int blogId, int parentCommentId, int userId, ReplyBlogCommentReq request, CancellationToken ct = default);
    Task<BaseResponse> GetBlogCommentsAsync(int blogId, CancellationToken ct = default);
}

