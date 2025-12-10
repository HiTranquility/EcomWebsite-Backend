using App.BLL.Dtos.BlogDto;
using App.BLL.Dtos.BlogDto.Requests;
using App.BLL.Services;
using App.UTIL.Abstractions.DTO.Response;
using App.UTIL.Extensions;
using App.UTIL.Helpers.Cache;
using App.UTIL.Helpers.Cache.Schemas;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/blogs")]
public class BlogController : ControllerBase
{
    private readonly BlogSvc _blogSvc;
    private readonly BlogCommentSvc _blogCommentSvc;
    public BlogController(BlogSvc blogSvc, BlogCommentSvc blogCommentSvc)
    {
        _blogSvc = blogSvc;
        _blogCommentSvc = blogCommentSvc;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetBlogList([FromQuery] BlogFilter filter, CancellationToken ct)
    {
        var rsp = await _blogSvc.GetBlogListAsync(filter, ct);
        return StatusCode(rsp.Status, rsp);
    }
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetBlogDetail(int id, CancellationToken ct)
    {
        var rsp = await _blogSvc.GetBlogDetailAsync(id, ct);
        return StatusCode(rsp.Status, rsp);
    }

    [AllowAnonymous]
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBlogDetailBySlug(string slug, CancellationToken ct)
    {
        var rsp = await _blogSvc.GetBlogDetailBySlugAsync(slug, ct);
        return StatusCode(rsp.Status, rsp);
    }

    [AllowAnonymous]
    [HttpGet("{blogId:int}/comments")]
    public async Task<IActionResult> GetBlogComments(int blogId, CancellationToken ct)
    {
        BaseResponse rsp = await _blogCommentSvc.GetBlogCommentsAsync(blogId, ct);
        return StatusCode(rsp.Status, rsp);
    }
    [HttpPost("{blogId:int}/comments")]
    public async Task<IActionResult> CreateBlogComment(
        int blogId,
        [FromBody] CreateBlogCommentReq request,
        CancellationToken ct)
    {
        int? userId = User.GetUserId();

        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        BaseResponse rsp = await _blogCommentSvc.CreateBlogCommentAsync(blogId, userId.Value, request, ct);
        return StatusCode(rsp.Status, rsp);
    }

    [HttpPost("{blogId:int}/comments/{parentCommentId:int}/reply")]
    public async Task<IActionResult> ReplyComment(
        int blogId,
        int parentCommentId,
        [FromBody] ReplyBlogCommentReq request,
        CancellationToken ct)
    {
        int? userId = User.GetUserId();

        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        BaseResponse rsp = await _blogCommentSvc.ReplyBlogCommentAsync(blogId, parentCommentId, userId.Value, request, ct);
        return StatusCode(rsp.Status, rsp);
    }
}