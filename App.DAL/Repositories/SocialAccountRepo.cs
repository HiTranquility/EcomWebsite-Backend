using App.DAL.UserModels;
using App.UTIL.Abstractions.DAL;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.Repositories;

public class SocialAccountRepo : GenericRepo<EcomUsersContext, SocialAccount>
{
    public SocialAccountRepo(EcomUsersContext context) : base(context)
    {
    }

    public Task<SocialAccount?> FindByProviderAsync(string provider, string providerUserId, CancellationToken ct = default)
    {
        return _context.SocialAccounts
            .AsNoTracking()
            .Include(sa => sa.User)
            .ThenInclude(u => u.Roles)
            .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(
                sa => sa.Provider == provider 
                   && sa.ProviderUserId == providerUserId 
                   && sa.DeletedAt == null,
                ct);
    }

    public Task<List<SocialAccount>> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return _context.SocialAccounts
            .AsNoTracking()
            .Where(sa => sa.UserId == userId && sa.DeletedAt == null)
            .ToListAsync(ct);
    }
}

