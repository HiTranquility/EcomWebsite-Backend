using App.DAL.UserModels;
using App.UTIL.Abstractions.DAL;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.Repositories;

public class RefreshTokenRepo : GenericRepo<EcomUsersContext, RefreshToken>
{
    public RefreshTokenRepo(EcomUsersContext context) : base(context)
    {
    }

    public Task<RefreshToken?> FindByHashAsync(string hash, CancellationToken ct = default)
    {
        return _context.RefreshTokens
            .AsNoTracking()
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.TokenHash == hash, ct);
    }

    public Task<List<RefreshToken>> GetActiveByUserAsync(int userId, CancellationToken ct = default)
    {
        return _context.RefreshTokens
            .AsNoTracking()
            .Where(r => r.UserId == userId && r.RevokedAt == null && r.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(ct);
    }
}