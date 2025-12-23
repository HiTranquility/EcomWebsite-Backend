using App.DAL.UserModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.Interfaces;

public interface IRefreshTokenRepo : IGenericRepo<RefreshToken>
{
    Task<RefreshToken?> FindByHashAsync(string hash, CancellationToken ct = default);
    Task<List<RefreshToken>> GetActiveByUserAsync(int userId, CancellationToken ct = default);
}

