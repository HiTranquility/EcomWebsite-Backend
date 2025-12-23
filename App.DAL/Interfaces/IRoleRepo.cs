using App.DAL.UserModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.Interfaces;

public interface IRoleRepo : IGenericRepo<Role>
{
    Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken ct = default);
}

