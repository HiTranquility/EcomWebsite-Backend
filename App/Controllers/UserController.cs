using App.BLL.Services;
using App.BLL.Dtos.UserDto.Requests;
using App.UTIL.Abstractions.DTO.Response;
using App.UTIL.Extensions;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
public class UserController : ControllerBase
{
    private readonly UserSvc _userSvc;

    public UserController(UserSvc userSvc)
    {
        _userSvc = userSvc;
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken ct = default)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _userSvc.GetUserProfileAsync(userId.Value, ct);
        return StatusCode(result.Status, result);
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileReq request, CancellationToken ct = default)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _userSvc.UpdateUserProfileAsync(userId.Value, request, ct);
        return StatusCode(result.Status, result);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordReq request, CancellationToken ct = default)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _userSvc.ChangePasswordAsync(userId.Value, request, ct);
        return StatusCode(result.Status, result);
    }
}

