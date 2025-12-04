using App.DAL.ProductModels;
using App.UTIL.Abstractions.DAL;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.Repositories;

public class ProductCategoryRepo : GenericRepo<EcomProductsContext, ProductCategory>
{
    public ProductCategoryRepo(EcomProductsContext context) : base(context)
    {
    }
    public async Task<List<(int CategoryId, int Total)>> GetCategoryCountsAsync(CancellationToken ct = default)
    {
        return await _context.Products
            .Where(p => p.DeletedAt == null && p.ProductCategoryId.HasValue)
            .GroupBy(p => p.ProductCategoryId!.Value)
            .Select(g => new ValueTuple<int, int>(g.Key, g.Count()))
            .ToListAsync(ct);
    }

    public async Task<List<ProductCategory>> GetCategoryTreeWithCountsAsync(CancellationToken ct = default)
    {
        return await _context.ProductCategories
            .Where(c => c.DeletedAt == null && c.Parent == null)
            .Include(c => c.InverseParentNavigation
                .Where(child => child.DeletedAt == null)
                .OrderBy(child => child.Title))
            .AsNoTracking()
            .ToListAsync(ct);
    }
}