using App.DAL.UserModels;
using App.UTIL.Abstractions.DAL;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.Repositories;

public class UserTagRepo : GenericRepo<EcomUsersContext, UserTag>
{
    public UserTagRepo(EcomUsersContext context) : base(context)
    {
    }

    public Task<List<UserTag>> GetProductTagsAsync(int productId, CancellationToken ct = default)
    {
        return _context.UserTags
            .AsNoTracking()
            .Where(t => t.ProductId == productId && t.DeletedAt == null)
            .OrderBy(t => t.Title)
            .ToListAsync(ct);
    }

    public Task<UserTag?> GetUserProductTagAsync(int userId, int productId, string tagTitle, CancellationToken ct = default)
    {
        return _context.UserTags
            .FirstOrDefaultAsync(t =>
                t.UserId == userId &&
                t.ProductId == productId &&
                t.Title == tagTitle &&
                t.DeletedAt == null, ct);
    }
}

