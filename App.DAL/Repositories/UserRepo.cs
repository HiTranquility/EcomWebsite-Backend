using App.DAL.UserModels;
using App.UTIL.Abstractions.DAL;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.Repositories;

public class UserRepo : GenericRepo<EcomUsersContext, User>
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
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }
}