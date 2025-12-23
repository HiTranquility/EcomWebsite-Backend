using App.DAL.Interfaces;
using App.DAL.UserModels;
using App.UTIL.Abstractions.DAL;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.Repositories;

public class UserRepo : GenericRepo<EcomUsersContext, User>, IUserRepo
{
    public UserRepo(EcomUsersContext context) : base(context)
    {
    }
    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        return _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == email, ct);
    }

    public Task<User?> FindByEmailAsync(string email, CancellationToken ct = default)
    {
        return _context.Users
            .AsNoTracking()
            .Include(u => u.Roles)
            .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }
    public override Task<User?> ReadAsync(int id, CancellationToken ct = default)
    {
        return _context.Users
            .AsNoTracking()
            .Include(u => u.Roles)
            .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public Task<Dictionary<int, User>> GetUsersByIdsAsync(IEnumerable<int> userIds, CancellationToken ct = default)
    {
        List<int> userIdList = userIds.Distinct().ToList();
        if (userIdList.Count == 0)
        {
            return Task.FromResult(new Dictionary<int, User>());
        }

        return _context.Users
            .AsNoTracking()
            .Where(u => userIdList.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, ct);
    }

    public Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken ct = default)
    {
        return _context.Roles
            .AsTracking()
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Name == roleName, ct);
    }

    public Task UpdateRememberPreferenceAsync(int userId, bool rememberMe, CancellationToken ct = default)
    {
        return _context.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(u => u.IsRemember, rememberMe)
                    .SetProperty(u => u.UpdatedAt, DateTime.UtcNow),
                ct);
    }

    public Task UpdateAvatarAsync(int userId, string? avatarUrl, DateTime updatedAt, CancellationToken ct = default)
    {
        return _context.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(u => u.ImageUrl, avatarUrl)
                    .SetProperty(u => u.UpdatedAt, updatedAt),
                ct);
    }

    /// <summary>
    /// Legacy method for backward compatibility. Use GetOrCreateSocialUserAsync instead.
    /// </summary>
    [Obsolete("Use GetOrCreateSocialUserAsync with provider='Google' instead")]
    public async Task<User?> GetOrCreateGoogleUserAsync(string email, string name, string picture, CancellationToken ct = default)
    {
        // Fallback: tìm user theo email (không dùng SocialAccount)
        var user = await FindByEmailAsync(email, ct);

        if (user != null)
        {
            if (!string.IsNullOrWhiteSpace(picture) && user.ImageUrl != picture)
            {
                await UpdateAvatarAsync(user.Id, picture, DateTime.UtcNow, ct);
                user = await ReadAsync(user.Id, ct);
            }
            return user;
        }

        var nameParts = name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var firstName = nameParts.Length > 0 ? nameParts[0] : name;
        var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

        var newUser = new User
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            ImageUrl = picture,
            IsActive = true,
            IsRemember = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await CreateAsync(newUser, ct);
        return await ReadAsync(newUser.Id, ct);
    }

    /// <summary>
    /// Tìm hoặc tạo User từ Social Account (Google, Facebook, etc.)
    /// Flow:
    /// 1. Tìm SocialAccount theo Provider + ProviderUserId
    /// 2. Nếu có → return User
    /// 3. Nếu không → tìm User theo email
    /// 4. Nếu có User → tạo SocialAccount và link
    /// 5. Nếu không có User → tạo User mới + SocialAccount
    /// </summary>
    public async Task<User?> GetOrCreateSocialUserAsync(
        SocialAccountRepo socialAccountRepo,
        string provider,
        string providerUserId,
        string email,
        string name,
        string? pictureUrl = null,
        string? accessToken = null,
        DateTime? accessTokenExpiresAt = null,
        CancellationToken ct = default)
    {
        // 1. Tìm SocialAccount đã tồn tại
        var existingSocialAccount = await socialAccountRepo.FindByProviderAsync(provider, providerUserId, ct);
        if (existingSocialAccount != null && existingSocialAccount.User != null)
        {
            var user = existingSocialAccount.User;
            
            // Update avatar nếu có thay đổi
            if (!string.IsNullOrWhiteSpace(pictureUrl) && user.ImageUrl != pictureUrl)
            {
                await UpdateAvatarAsync(user.Id, pictureUrl, DateTime.UtcNow, ct);
                user = await ReadAsync(user.Id, ct);
            }
            
            // Update access token nếu có
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                existingSocialAccount.AccessToken = accessToken;
                existingSocialAccount.AccessTokenExpiresAt = accessTokenExpiresAt;
                existingSocialAccount.UpdatedAt = DateTime.UtcNow;
                await socialAccountRepo.UpdateAsync(existingSocialAccount, ct);
            }
            
            return user;
        }

        // 2. Tìm User theo email (có thể đã có account nhưng chưa link social)
        var userByEmail = await FindByEmailAsync(email, ct);
        User targetUser;

        if (userByEmail != null)
        {
            // User đã tồn tại → tạo SocialAccount và link
            targetUser = userByEmail;
        }
        else
        {
            // 3. Tạo User mới
            var nameParts = name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var firstName = nameParts.Length > 0 ? nameParts[0] : name;
            var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

            targetUser = new User
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                ImageUrl = pictureUrl,
                IsActive = true,
                IsRemember = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await CreateAsync(targetUser, ct);
        }

        // 4. Tạo SocialAccount và link với User
        var socialAccount = new SocialAccount
        {
            UserId = targetUser.Id,
            Provider = provider,
            ProviderUserId = providerUserId,
            Email = email,
            Name = name,
            PictureUrl = pictureUrl,
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await socialAccountRepo.CreateAsync(socialAccount, ct);

        // 5. Reload user với roles và permissions
        return await ReadAsync(targetUser.Id, ct);
    }
}