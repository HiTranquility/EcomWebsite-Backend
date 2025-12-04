using App.BLL.Services;
using Asp.Versioning;
using App.BLL.Dtos.AuthDto.Requests.Google;
using App.BLL.Dtos.AuthDto.Requests.Facebook;
using App.BLL.Dtos.AuthDto.Requests.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using App.UTIL.Extensions;

namespace App.Controllers;
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthSvc _authSvc;

    public AuthController(AuthSvc authSvc)
    {
        _authSvc = authSvc;
    }

    [AllowAnonymous]    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginReq request, CancellationToken ct)
    {
        var rsp = await _authSvc.LoginAsync(request, HttpContext.GetClientIp(), ct);
        return StatusCode(rsp.Status, rsp);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterReq request, CancellationToken ct)
    {
        var rsp = await _authSvc.RegisterAsync(request, HttpContext.GetClientIp(), ct);
        return StatusCode(rsp.Status, rsp);
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutReq? request, CancellationToken ct)
    {
        var rsp = await _authSvc.LogoutAsync(request, HttpContext.GetRefreshToken(), HttpContext.GetClientIp(), ct);
        return StatusCode(rsp.Status, rsp);
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenReq request, CancellationToken ct)
    {
        var rsp = await _authSvc.RefreshTokenAsync(request, HttpContext.GetClientIp(), ct);
        return StatusCode(rsp.Status, rsp);
    }

    [AllowAnonymous]
    [HttpPost("google/callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] CallbackReq request, CancellationToken ct)
    {
        var rsp = await _authSvc.GoogleCallbackAsync(request, HttpContext.GetClientIp(), ct);
        return StatusCode(rsp.Status, rsp);
    }

    [AllowAnonymous]
    [HttpPost("google/id-token")]
    public async Task<IActionResult> GoogleIdToken([FromBody] IdTokenReq request, CancellationToken ct)
    {
        var rsp = await _authSvc.GoogleIdTokenAsync(request, HttpContext.GetClientIp(), ct);
        return StatusCode(rsp.Status, rsp);
    }

    [AllowAnonymous]
    [HttpPost("facebook/access-token")]
    public async Task<IActionResult> FacebookAsync([FromBody] AccessTokenReq request, CancellationToken ct)
    {
        var rsp = await _authSvc.FacebookAsync(request, HttpContext.GetClientIp(), ct);
        return StatusCode(rsp.Status, rsp);
    }
}