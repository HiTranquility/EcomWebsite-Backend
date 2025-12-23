using App.DAL.Interfaces;
using App.UTIL.Abstractions.DAL;
using App.DAL.UserModels;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.Repositories;

public class RoleRepo : GenericRepo<EcomUsersContext, Role>, IRoleRepo
{
    public RoleRepo(EcomUsersContext context) : base(context)
    {
    }

    public Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken ct = default)
    {
        return _context.Roles
            .AsTracking()
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Name == roleName, ct);
    }
}