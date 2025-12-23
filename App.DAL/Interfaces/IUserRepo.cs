using App.DAL.Repositories;
using App.DAL.UserModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.Interfaces;

public interface IUserRepo : IGenericRepo<User>
{
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> FindByEmailAsync(string email, CancellationToken ct = default);
    Task<Dictionary<int, User>> GetUsersByIdsAsync(IEnumerable<int> userIds, CancellationToken ct = default);
    Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken ct = default);
    Task UpdateRememberPreferenceAsync(int userId, bool rememberMe, CancellationToken ct = default);
    Task UpdateAvatarAsync(int userId, string? avatarUrl, DateTime updatedAt, CancellationToken ct = default);
    
    Task<User?> GetOrCreateSocialUserAsync(
        SocialAccountRepo socialAccountRepo,
        string provider,
        string providerUserId,
        string email,
        string name,
        string? pictureUrl = null,
        string? accessToken = null,
        DateTime? accessTokenExpiresAt = null,
        CancellationToken ct = default);
}

