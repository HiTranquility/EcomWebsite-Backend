using App.BLL.Dtos.AuthDto;
using App.BLL.Dtos.AuthDto.Results;
using App.BLL.Dtos.AuthDto.Requests;
using App.DAL.UserModels;
using App.DAL.Repositories;
using App.UTIL.Abstractions.BLL;
using App.UTIL.Abstractions.DTO.Response;
using App.UTIL.Helpers.Jwt;

using AutoMapper;

namespace App.BLL.Services;

public class AuthSvc : GenericSvc<UserRepo, User>
{
    private readonly IMapper _mapper;
    private readonly IJwtService _jwtService;
    
    public AuthSvc(UserRepo repo, IMapper mapper, IJwtService jwtService) : base(repo)
    {
        _mapper = mapper;
        _jwtService = jwtService;
    }

    public async Task<BaseResponse> RegisterAsync(RegisterReq request, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();
        if (await _repo.ExistsByEmailAsync(request.Email, ct))
        {
            rsp.SetError("EMAIL_EXISTS", "Email already exists", "BUSINESS_ERROR", 400);
            return rsp;
        }

        var user = _mapper.Map<User>(request);
        await _repo.CreateAsync(user);
        rsp.SetData(user, "Register successfully", 201);
        return rsp;
    }

    public async Task<BaseResponse> LoginAsync(LoginReq request, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            rsp.SetError("INVALID_CREDENTIALS", "Email and password are required", "Validation failed", 400);
            return rsp;
        }

        var email = request.Email.Trim();
        var user = await _repo.FindByEmailAsync(email, ct);
        if (user == null)
        {
            rsp.SetError("USER_NOT_FOUND", "User not found", "Authentication failed", 401);
            return rsp;
        }

        // NOTE: Demo only. In production, verify hashed password.
        if (!string.Equals(user.PasswordHash, request.Password))
        {
            rsp.SetError("INVALID_PASSWORD", "Invalid password", "Authentication failed", 401);
            return rsp;
        }

        var token = _jwtService.GenerateToken(user.Id.ToString(), user.Role ?? "user", TimeSpan.FromHours(2));
        rsp.SetData(new { token }, "Login Successfully", 200);
        return rsp;
    }

    public async Task<BaseResponse> ForgotPasswordAsync(ForgotPasswordReq request, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            rsp.SetError("EMAIL_REQUIRED", "Email is required", "Validation failed", 400);
            return rsp;
        }

        var email = request.Email.Trim();
        var _ = await _repo.FindByEmailAsync(email, ct); // optional existence check

        // Không tiết lộ thông tin tồn tại của email
        rsp.SetMessage("If the email exists, reset instructions have been sent.", 200, success: true);
        return rsp;
    }

    public async Task<BaseResponse> ResetPasswordAsync(ResetPasswordReq request, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        //TODO: Nghiên cứu lại, vì ở đây request.Email đã đc validation từ DataAnnotations
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            rsp.SetError("INVALID_INPUT", "Email, token and new password are required", "Validation failed", 400);
            return rsp;
        }

        var email = request.Email.Trim();
        var user = await _repo.FindByEmailAsync(email, ct);
        if (user == null)
        {
            // Không tiết lộ thông tin người dùng
            rsp.SetMessage("Password reset successfully", 200, success: true);
            return rsp;
        }

        // Demo only: chưa xác thực token và hashing password
        // user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        await _repo.UpdateAsync(user, ct);

        rsp.SetMessage("Password reset successfully", 200, success: true);
        return rsp;
    }

    public async Task<BaseResponse> GetProfileAsync(int userId, string role, CancellationToken ct = default)
    {
        var user = await _repo.ReadAsync(userId);
        var payload = _mapper.Map<ProfileRes>(user);
        return new BaseResponse(payload, "Get profile successfully", 200);
    }
}