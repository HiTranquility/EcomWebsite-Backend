using App.BLL.Dtos.BlogDto;
using App.BLL.Dtos.BlogDto.Requests;
using App.BLL.Interfaces;
using App.UTIL.Abstractions.DTO.Response;
using App.UTIL.Extensions;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

/// <summary>
/// Blog Controller - Handles blog posts and comments
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/blogs")]
public class BlogController : ControllerBase
{
    private readonly IBlogSvc _blogSvc;
    private readonly IBlogCommentSvc _blogCommentSvc;
    
    public BlogController(IBlogSvc blogSvc, IBlogCommentSvc blogCommentSvc)
    {
        _blogSvc = blogSvc;
        _blogCommentSvc = blogCommentSvc;
    }

    /// <summary>
    /// Get blog list with filters
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBlogList([FromQuery] BlogFilter filter, CancellationToken ct)
    {
        var rsp = await _blogSvc.GetBlogListAsync(filter, ct);
        return StatusCode(rsp.Status, rsp);
    }

    /// <summary>
    /// Get blog detail by ID
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBlogDetail(int id, CancellationToken ct)
    {
        var rsp = await _blogSvc.GetBlogDetailAsync(id, ct);
        return StatusCode(rsp.Status, rsp);
    }

    /// <summary>
    /// Get blog detail by slug
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{slug}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBlogDetailBySlug(string slug, CancellationToken ct)
    {
        var rsp = await _blogSvc.GetBlogDetailBySlugAsync(slug, ct);
        return StatusCode(rsp.Status, rsp);
    }

    /// <summary>
    /// Get comments for a blog post
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{blogId:int}/comments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBlogComments(int blogId, CancellationToken ct)
    {
        BaseResponse rsp = await _blogCommentSvc.GetBlogCommentsAsync(blogId, ct);
        return StatusCode(rsp.Status, rsp);
    }

    /// <summary>
    /// Create a new comment on a blog post
    /// </summary>
    [Authorize]
    [HttpPost("{blogId:int}/comments")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Reply to an existing comment
    /// </summary>
    [Authorize]
    [HttpPost("{blogId:int}/comments/{parentCommentId:int}/reply")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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