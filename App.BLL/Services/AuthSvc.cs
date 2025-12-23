using App.UTIL.Abstractions.BLL;
using App.DAL.Repositories;
using App.DAL.UserModels;
using App.UTIL.Abstractions.DAL;
using AutoMapper;
using App.BLL.Dtos.AuthDto.Requests.Jwt;
using GoogleReq = App.BLL.Dtos.AuthDto.Requests.Google;
using FacebookReq = App.BLL.Dtos.AuthDto.Requests.Facebook;
using App.BLL.Dtos.AuthDto.Results;
using App.BLL.Dtos.AuthDto.Shares;
using App.UTIL.Abstractions.DTO.Response;
using App.UTIL.Extensions;
using App.INFRA.Identity;
using App.INFRA.ExternalAuth.Google;
using App.INFRA.ExternalAuth.Facebook;

using App.BLL.Interfaces;

namespace App.BLL.Services;

public class AuthSvc : GenericSvc<UserRepo, User>, IAuthSvc
{
    private readonly ITokenService _tokenService;
    private readonly IGoogleService _googleService;
    private readonly IFacebookService? _facebookService; // Optional - may not be configured
    private readonly SocialAccountRepo _socialAccountRepo;
    private readonly RefreshTokenRepo _refreshTokenRepo;

    public AuthSvc(
        UserRepo repo,
        ITokenService tokenService,
        IGoogleService googleService,
        IFacebookService? facebookService, // Optional dependency
        SocialAccountRepo socialAccountRepo,
        RefreshTokenRepo refreshTokenRepo,
        IMapper mapper) : base(repo, mapper)
    {
        _tokenService = tokenService;
        _googleService = googleService;
        _facebookService = facebookService;
        _socialAccountRepo = socialAccountRepo;
        _refreshTokenRepo = refreshTokenRepo;
    }


    public async Task<BaseResponse> LoginAsync(LoginReq request, string? clientIp = null, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            rsp.SetError("INVALID_REQUEST", "Email and password are required", "Invalid request", 400);
            return rsp;
        }

        var user = await _repo.FindByEmailAsync(request.Email, ct);
        if (user == null || !PasswordHasherExtensions.Verify(request.Password, user.PasswordHash ?? string.Empty))
        {
            rsp.SetError("INVALID_CREDENTIALS", "Invalid credentials", "Authentication failed", 401);
            return rsp;
        }

        if (user.IsActive != true)
        {
            rsp.SetError("EMAIL_NOT_VERIFIED", "Email address is not verified", "Email verification required", 403);
            return rsp;
        }

        if (user.IsRemember != request.RememberMe)
        {
            await _repo.UpdateRememberPreferenceAsync(user.Id, request.RememberMe, ct);
        }

        var roles = user.Roles?.Select(r => r.Name).Where(n => !string.IsNullOrWhiteSpace(n)).ToList() ?? new List<string>();
        var permissions = user.Roles?
            .SelectMany(r => r.Permissions ?? Enumerable.Empty<Permission>())
            .Select(p => p.Key)
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Distinct()
            .ToList() ?? new List<string>();

        var accessToken = _tokenService.GenerateAccessToken(
            user.Id.ToString(),
            roles,
            permissions
        );

        var (refreshToken, _, expiresAt) = _tokenService.GenerateRefreshToken(user.Id.ToString(), request.RememberMe);
        var tokenHash = TokenHasherExtensions.HashToken(refreshToken);

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt.UtcDateTime,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = clientIp
        };

        await _refreshTokenRepo.CreateAsync(refreshTokenEntity, ct);

        var userInfo = _mapper?.Map<UserInfoItem>(user);

        var session = new AuthSessionRes
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenType = "Bearer",
            User = userInfo ?? new UserInfoItem(),
            Roles = roles,
            Permissions = permissions
        };
        rsp.SetData(session, "Login successful", 200);
        return rsp;
    }

    public async Task<BaseResponse> RegisterAsync(RegisterReq request, string? clientIp = null, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        if (string.IsNullOrWhiteSpace(request.Email)
            || string.IsNullOrWhiteSpace(request.Password)
            || string.IsNullOrWhiteSpace(request.FirstName)
            || string.IsNullOrWhiteSpace(request.LastName))
        {
            rsp.SetError("INVALID_REQUEST", "Missing required fields", "Invalid request", 400);
            return rsp;
        }

        var email = request.Email.Trim();
        if (await _repo.ExistsByEmailAsync(email, ct))
        {
            rsp.SetError("EMAIL_EXISTS", "Email is already registered", "Email already exists", 409);
            return rsp;
        }

        var newUser = new User
        {
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            Gender = request.Gender,
            Birthday = request.Birthday,
            PasswordHash = PasswordHasherExtensions.Hash(request.Password),
            IsActive = true,
            IsSubscribe = request.IsSubscribe ?? false,
            IsRemember = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var defaultRole = await _repo.GetRoleByNameAsync("Customer", ct);
        if (defaultRole != null)
        {
            newUser.Roles.Add(defaultRole);
        }

        await _repo.CreateAsync(newUser, ct);
        var user = await _repo.ReadAsync(newUser.Id, ct) ?? newUser;

        var roles = user.Roles?.Select(r => r.Name).Where(n => !string.IsNullOrWhiteSpace(n)).ToList() ?? new List<string>();
        var permissions = user.Roles?
            .SelectMany(r => r.Permissions ?? Enumerable.Empty<Permission>())
            .Select(p => p.Key)
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Distinct()
            .ToList() ?? new List<string>();

        var accessToken = _tokenService.GenerateAccessToken(
            user.Id.ToString(),
            roles,
            permissions
        );

        var (refreshToken, _, expiresAt) = _tokenService.GenerateRefreshToken(user.Id.ToString(), false);
        var tokenHash = TokenHasherExtensions.HashToken(refreshToken);

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt.UtcDateTime,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = clientIp
        };

        await _refreshTokenRepo.CreateAsync(refreshTokenEntity, ct);

        var userInfo = _mapper?.Map<UserInfoItem>(user);

        var session = new AuthSessionRes
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenType = "Bearer",
            User = userInfo ?? new UserInfoItem(),
            Roles = roles,
            Permissions = permissions
        };

        rsp.SetData(session, "Registration successful", 200);
        return rsp;
    }

    public async Task<BaseResponse> LogoutAsync(LogoutReq? request = null, string? refreshToken = null, string? clientIp = null, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        // Get refresh token from parameter or request body
        var token = refreshToken ?? request?.RefreshToken;

        if (!string.IsNullOrWhiteSpace(token))
        {
            var tokenHash = TokenHasherExtensions.HashToken(token);
            var tokenEntity = await _refreshTokenRepo.FindByHashAsync(tokenHash, ct);

            if (tokenEntity != null && tokenEntity.RevokedAt == null)
            {
                tokenEntity.RevokedAt = DateTime.UtcNow;
                tokenEntity.RevokedByIp = clientIp;
                await _refreshTokenRepo.UpdateAsync(tokenEntity, ct);
            }
        }

        rsp.SetData(null, "Logout successful", 200);
        return rsp;
    }

    public async Task<BaseResponse> RefreshTokenAsync(RefreshTokenReq request, string? clientIp = null, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            rsp.SetError("INVALID_REQUEST", "Refresh token is required", "Invalid request", 400);
            return rsp;
        }

        var tokenHash = TokenHasherExtensions.HashToken(request.RefreshToken);
        var tokenEntity = await _refreshTokenRepo.FindByHashAsync(tokenHash, ct);

        if (tokenEntity == null)
        {
            rsp.SetError("INVALID_TOKEN", "Invalid refresh token", "Token not found", 401);
            return rsp;
        }

        if (tokenEntity.RevokedAt != null)
        {
            rsp.SetError("TOKEN_REVOKED", "Refresh token has been revoked", "Token revoked", 401);
            return rsp;
        }

        if (tokenEntity.ExpiresAt < DateTime.UtcNow)
        {
            rsp.SetError("TOKEN_EXPIRED", "Refresh token has expired", "Token expired", 401);
            return rsp;
        }

        var user = tokenEntity.User;
        if (user == null || user.IsActive != true)
        {
            rsp.SetError("USER_INACTIVE", "User is inactive", "User account is inactive", 403);
            return rsp;
        }

        // Revoke old token
        tokenEntity.RevokedAt = DateTime.UtcNow;
                tokenEntity.RevokedByIp = clientIp;
        tokenEntity.ReplacedByTokenHash = null; // Will be set after creating new token
        await _refreshTokenRepo.UpdateAsync(tokenEntity, ct);

        var rememberMe = user.IsRemember ?? false;
        var roles = user.Roles?.Select(r => r.Name).Where(n => !string.IsNullOrWhiteSpace(n)).ToList() ?? new List<string>();
        var permissions = user.Roles?
            .SelectMany(r => r.Permissions ?? Enumerable.Empty<Permission>())
            .Select(p => p.Key)
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Distinct()
            .ToList() ?? new List<string>();

        var accessToken = _tokenService.GenerateAccessToken(
            user.Id.ToString(),
            roles,
            permissions
        );

        var (newRefreshToken, _, expiresAt) = _tokenService.GenerateRefreshToken(user.Id.ToString(), rememberMe);
        var newTokenHash = TokenHasherExtensions.HashToken(newRefreshToken);

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = newTokenHash,
            ExpiresAt = expiresAt.UtcDateTime,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = clientIp
        };

        await _refreshTokenRepo.CreateAsync(refreshTokenEntity, ct);

        var userInfo = _mapper?.Map<UserInfoItem>(user);

        var session = new AuthSessionRes
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            TokenType = "Bearer",
            User = userInfo ?? new UserInfoItem(),
            Roles = roles,
            Permissions = permissions
        };

        // Update old token with new token hash
        tokenEntity.ReplacedByTokenHash = newTokenHash;
        await _refreshTokenRepo.UpdateAsync(tokenEntity, ct);

        rsp.SetData(session, "Token refreshed successfully", 200);
        return rsp;
    }

    private async Task<AuthSessionRes> CreateSessionAsync(User user, string? clientIp = null, CancellationToken ct = default)
    {
        var roles = user.Roles?.Select(r => r.Name).Where(n => !string.IsNullOrWhiteSpace(n)).ToList() ?? new List<string>();
        var permissions = user.Roles?
            .SelectMany(r => r.Permissions ?? Enumerable.Empty<Permission>())
            .Select(p => p.Key)
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Distinct()
            .ToList() ?? new List<string>();

        var accessToken = _tokenService.GenerateAccessToken(
            user.Id.ToString(),
            roles,
            permissions
        );

        var (refreshToken, _, expiresAt) = _tokenService.GenerateRefreshToken(user.Id.ToString(), false);
        var tokenHash = TokenHasherExtensions.HashToken(refreshToken);

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt.UtcDateTime,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = clientIp
        };

        await _refreshTokenRepo.CreateAsync(refreshTokenEntity, ct);

        var userInfo = _mapper?.Map<UserInfoItem>(user);

        return new AuthSessionRes
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenType = "Bearer",
            User = userInfo ?? new UserInfoItem(),
            Roles = roles,
            Permissions = permissions
        };
    }

    public async Task<BaseResponse> GoogleCallbackAsync(GoogleReq.CallbackReq request, string? clientIp = null, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        if (string.IsNullOrWhiteSpace(request.Code))
        {
            rsp.SetError("INVALID_REQUEST", "Authorization code is required", "Invalid request", 400);
            return rsp;
        }

        var (success, email, name, picture, _) = await _googleService.ExchangeCodeAsync(request.Code, ct);

        if (!success || string.IsNullOrWhiteSpace(email))
        {
            rsp.SetError("GOOGLE_AUTH_FAILED", "Failed to authenticate with Google", "Google authentication failed", 401);
            return rsp;
        }

        // Lấy Google User ID từ token (cần parse từ id_token)
        // Tạm thời dùng email làm providerUserId, nhưng tốt hơn là lấy từ token
        var providerUserId = email; // TODO: Extract actual Google sub from token
        
        var user = await _repo.GetOrCreateSocialUserAsync(
            _socialAccountRepo,
            provider: "Google",
            providerUserId: providerUserId,
            email: email,
            name: name,
            pictureUrl: picture,
            ct: ct);

        if (user == null || user.IsActive != true)
        {
            rsp.SetError("USER_INACTIVE", "User is inactive", "User account is inactive", 403);
            return rsp;
        }

        var session = await CreateSessionAsync(user, clientIp, ct);
        rsp.SetData(session, "Google authentication successful", 200);
        return rsp;
    }

    public async Task<BaseResponse> GoogleIdTokenAsync(GoogleReq.IdTokenReq request, string? clientIp = null, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        if (string.IsNullOrWhiteSpace(request.IdToken))
        {
            rsp.SetError("INVALID_REQUEST", "ID token is required", "Invalid request", 400);
            return rsp;
        }

        var (success, email, name, picture, _) = await _googleService.ValidateIdTokenAsync(request.IdToken, ct);

        if (!success || string.IsNullOrWhiteSpace(email))
        {
            rsp.SetError("GOOGLE_AUTH_FAILED", "Failed to validate Google ID token", "Google token validation failed", 401);
            return rsp;
        }

        // Lấy Google User ID từ token (cần parse từ id_token)
        // Tạm thời dùng email làm providerUserId, nhưng tốt hơn là lấy từ token
        var providerUserId = email; // TODO: Extract actual Google sub from token
        
        var user = await _repo.GetOrCreateSocialUserAsync(
            _socialAccountRepo,
            provider: "Google",
            providerUserId: providerUserId,
            email: email,
            name: name,
            pictureUrl: picture,
            ct: ct);

        if (user == null || user.IsActive != true)
        {
            rsp.SetError("USER_INACTIVE", "User is inactive", "User account is inactive", 403);
            return rsp;
        }

        var session = await CreateSessionAsync(user, clientIp, ct);
        rsp.SetData(session, "Google authentication successful", 200);
        return rsp;
    }

    public async Task<BaseResponse> FacebookAsync(FacebookReq.AccessTokenReq request, string? clientIp = null, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        // Check if Facebook auth is configured
        if (_facebookService == null)
        {
            rsp.SetError("FACEBOOK_NOT_CONFIGURED", "Facebook authentication is not configured", "Facebook auth unavailable", 503);
            return rsp;
        }

        if (string.IsNullOrWhiteSpace(request.AccessToken))
        {
            rsp.SetError("INVALID_REQUEST", "Access token is required", "Invalid request", 400);
            return rsp;
        }

        var userInfo = await _facebookService.GetUserInfoAsync(request.AccessToken, ct);

        if (userInfo == null || string.IsNullOrWhiteSpace(userInfo.Email))
        {
            rsp.SetError("FACEBOOK_AUTH_FAILED", "Failed to authenticate with Facebook", "Facebook authentication failed", 401);
            return rsp;
        }

        var user = await _repo.GetOrCreateSocialUserAsync(
            _socialAccountRepo,
            provider: "Facebook",
            providerUserId: userInfo.Id,
            email: userInfo.Email,
            name: userInfo.Name,
            pictureUrl: userInfo.PictureUrl,
            accessToken: request.AccessToken,
            ct: ct);

        if (user == null || user.IsActive != true)
        {
            rsp.SetError("USER_INACTIVE", "User is inactive", "User account is inactive", 403);
            return rsp;
        }

        var session = await CreateSessionAsync(user, clientIp, ct);
        rsp.SetData(session, "Facebook authentication successful", 200);
        return rsp;
    }
}
