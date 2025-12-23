using App.DAL.UserModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.Interfaces;

public interface ISocialAccountRepo : IGenericRepo<SocialAccount>
{
    Task<SocialAccount?> FindByProviderAsync(string provider, string providerUserId, CancellationToken ct = default);
    Task<List<SocialAccount>> GetByUserIdAsync(int userId, CancellationToken ct = default);
}

