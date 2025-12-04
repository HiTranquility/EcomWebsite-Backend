using App.DAL.BlogModels;
using App.UTIL.Abstractions.DAL;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.Repositories;

public class BlogRepo : GenericRepo<EcomBlogsContext, Blog>
{
    public BlogRepo (EcomBlogsContext context) : base(context)
    {
    }

    // Override ReadAsync(string) để hỗ trợ tìm theo slug
    public override async Task<Blog?> ReadAsync(string slug, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return null;

        return await _context.Set<Blog>()
            .Where(b => b.Slug != null && b.Slug == slug && b.DeletedAt == null)
            .FirstOrDefaultAsync(ct);
    }
}