using App.BLL.Interfaces;
using Asp.Versioning;
using App.BLL.Dtos.AuthDto.Requests.Google;
using App.BLL.Dtos.AuthDto.Requests.Facebook;
using App.BLL.Dtos.AuthDto.Requests.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using App.UTIL.Extensions;

namespace App.Controllers;

/// <summary>
/// Auth Controller - Handles authentication operations (JWT, Google, Facebook)
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthSvc _authSvc;

    public AuthController(IAuthSvc authSvc)
    {
        _authSvc = authSvc;
    }

    /// <summary>
    /// User login with email and password
    /// </summary>
    [AllowAnonymous]    
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginReq request, CancellationToken ct)
    {
        var rsp = await _authSvc.LoginAsync(request, HttpContext.GetClientIp(), ct);
        return StatusCode(rsp.Status, rsp);
    }

    /// <summary>
    /// User registration
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterReq request, CancellationToken ct)
    {
        var rsp = await _authSvc.RegisterAsync(request, HttpContext.GetClientIp(), ct);
        return StatusCode(rsp.Status, rsp);
    }

    /// <summary>
    /// User logout
    /// </summary>
    [AllowAnonymous]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout([FromBody] LogoutReq? request, CancellationToken ct)
    {
        var rsp = await _authSvc.LogoutAsync(request, HttpContext.GetRefreshToken(), HttpContext.GetClientIp(), ct);
        return StatusCode(rsp.Status, rsp);
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenReq request, CancellationToken ct)
    {
        var rsp = await _authSvc.RefreshTokenAsync(request, HttpContext.GetClientIp(), ct);
        return StatusCode(rsp.Status, rsp);
    }

    /// <summary>
    /// Google OAuth callback handler
    /// </summary>
    [AllowAnonymous]
    [HttpPost("google/callback")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GoogleCallback([FromQuery] CallbackReq request, CancellationToken ct)
    {
        var rsp = await _authSvc.GoogleCallbackAsync(request, HttpContext.GetClientIp(), ct);
        return StatusCode(rsp.Status, rsp);
    }

    /// <summary>
    /// Google ID Token authentication (for mobile/SPA)
    /// </summary>
    [AllowAnonymous]
    [HttpPost("google/id-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GoogleIdToken([FromBody] IdTokenReq request, CancellationToken ct)
    {
        var rsp = await _authSvc.GoogleIdTokenAsync(request, HttpContext.GetClientIp(), ct);
        return StatusCode(rsp.Status, rsp);
    }

    /// <summary>
    /// Facebook access token authentication
    /// </summary>
    [AllowAnonymous]
    [HttpPost("facebook/access-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> FacebookAsync([FromBody] AccessTokenReq request, CancellationToken ct)
    {
        var rsp = await _authSvc.FacebookAsync(request, HttpContext.GetClientIp(), ct);
        return StatusCode(rsp.Status, rsp);
    }
}