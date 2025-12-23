using App.BLL.Interfaces;
using App.BLL.Dtos.UserDto.Requests;
using App.UTIL.Abstractions.DTO.Response;
using App.UTIL.Extensions;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

/// <summary>
/// User Controller - Handles user profile and account operations
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
public class UserController : ControllerBase
{
    private readonly IUserSvc _userSvc;

    public UserController(IUserSvc userSvc)
    {
        _userSvc = userSvc;
    }

    /// <summary>
    /// Get current user's profile
    /// </summary>
    [Authorize]
    [HttpGet("profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Update current user's profile
    /// </summary>
    [Authorize]
    [HttpPut("profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Change user's password
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
