using App.BLL.Dtos.BlogDto;
using App.BLL.Services;
using App.DAL.Repositories;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
namespace App.Controllers;
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/blog")]
public class BlogController : ControllerBase
{
    private readonly BlogSvc _blogSvc;
    public BlogController(BlogSvc blogSvc)
    {
        _blogSvc = blogSvc;
    }

    [HttpGet]
    public async Task<IActionResult> GetBlogLists([FromQuery] BlogFilter filter)
    {
        var rsp = await _blogSvc.GetBlogListsAsync(filter);
        return Ok(rsp);
    }
}