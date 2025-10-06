using App.BLL.Dtos.AuthDto.Requests;
using App.BLL.Services;
using App.UTIL.Extensions;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;

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
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterReq request, CancellationToken ct)
    {
        var rsp = await _authSvc.RegisterAsync(request, ct);
        return Ok(rsp);
    }
    
    /*[HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterReq request)
    {
        var res = await _authSvc.RegisterAsync(request);
        if (!res.Success)
        {
            return res.Status switch
            {
                400 => BadRequest(res),
                409 => Conflict(res),
                500 => StatusCode(500, res),
                _   => StatusCode(res.Status, res) // fallback cho an toàn
            };
        }
        return Ok(res);
    }*/
    
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginReq request, CancellationToken ct)
    {
        var rsp = await _authSvc.LoginAsync(request, ct);
        return Ok(rsp);
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordReq request, CancellationToken ct)
    {
        var rsp = await _authSvc.ResetPasswordAsync(request, ct);
        return Ok(rsp);
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordReq request, CancellationToken ct)
    {
        var rsp = await _authSvc.ForgotPasswordAsync(request, ct);
        return Ok(rsp);
    }

    [Authorize(Roles = "user")]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var rsp = await _authSvc.GetProfileAsync(User.GetUserId()!.Value, User.GetUserRole()!, ct);
        return Ok(rsp);
    }
    
}