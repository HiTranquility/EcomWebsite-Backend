using App.BLL.Dtos.UserDto.Requests;
using App.BLL.Dtos.UserDto.Results;
using App.DAL.Repositories;
using App.DAL.UserModels;
using App.UTIL.Abstractions.BLL;
using App.UTIL.Abstractions.DTO.Response;
using App.UTIL.Extensions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

using App.BLL.Interfaces;

namespace App.BLL.Services;

public class UserSvc : GenericSvc<UserRepo, User>, IUserSvc
{
    public UserSvc(UserRepo repo, IMapper mapper) : base(repo, mapper)
    {
    }

    public async Task<BaseResponse> GetUserProfileAsync(int userId, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        var user = await _repo.ReadAsync(userId, ct);
        if (user == null || user.DeletedAt != null)
        {
            rsp.SetError("USER_NOT_FOUND", "User not found", "User not found", 404);
            return rsp;
        }

        var mapped = _mapper.Map<UserProfileRes>(user);
        rsp.SetData(mapped, "Get user profile successfully", 200);
        return rsp;
    }

    public async Task<BaseResponse> UpdateUserProfileAsync(int userId, UpdateUserProfileReq request, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        var user = await _repo.All
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, ct);

        if (user == null)
        {
            rsp.SetError("USER_NOT_FOUND", "User not found", "User not found", 404);
            return rsp;
        }

        // Update fields
        if (!string.IsNullOrWhiteSpace(request.FirstName))
            user.FirstName = request.FirstName.Trim();
        if (!string.IsNullOrWhiteSpace(request.LastName))
            user.LastName = request.LastName.Trim();
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            user.PhoneNumber = request.PhoneNumber.Trim();
        if (request.Birthday.HasValue)
            user.Birthday = request.Birthday;
        if (!string.IsNullOrWhiteSpace(request.Gender))
            user.Gender = request.Gender.Trim();
        if (request.IsSubscribe.HasValue)
            user.IsSubscribe = request.IsSubscribe;
        user.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(user, ct);

        var mapped = _mapper.Map<UserProfileRes>(user);
        rsp.SetData(mapped, "Profile updated successfully", 200);
        return rsp;
    }

    public async Task<BaseResponse> ChangePasswordAsync(int userId, ChangePasswordReq request, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            rsp.SetError("INVALID_REQUEST", "Current password and new password are required", "Invalid request", 400);
            return rsp;
        }

        if (request.NewPassword.Length < 6)
        {
            rsp.SetError("INVALID_PASSWORD", "New password must be at least 6 characters", "Invalid password", 400);
            return rsp;
        }

        var user = await _repo.All
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, ct);

        if (user == null)
        {
            rsp.SetError("USER_NOT_FOUND", "User not found", "User not found", 404);
            return rsp;
        }

        // Verify current password
        if (string.IsNullOrWhiteSpace(user.PasswordHash) || !PasswordHasherExtensions.Verify(request.CurrentPassword, user.PasswordHash))
        {
            rsp.SetError("INVALID_PASSWORD", "Current password is incorrect", "Invalid password", 400);
            return rsp;
        }

        // Update password
        user.PasswordHash = PasswordHasherExtensions.Hash(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(user, ct);

        rsp.SetData(null, "Password changed successfully", 200);
        return rsp;
    }
}

