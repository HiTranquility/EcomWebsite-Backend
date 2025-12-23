using App.DAL.Interfaces;
using App.DAL.ProductModels;
using App.UTIL.Abstractions.DAL;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.Repositories;

public class ProductRepo : GenericRepo<EcomProductsContext, Product>, IProductRepo
{
    public ProductRepo(EcomProductsContext context) : base(context)
    {
    }

    // Override ReadAsync(string) để hỗ trợ tìm theo slug
    public override async Task<Product?> ReadAsync(string slug, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return null;

        return await _context.Set<Product>()
            .Where(p => p.Slug != null && p.Slug == slug && p.DeletedAt == null)
            .FirstOrDefaultAsync(ct);
    }
    public async Task<List<(decimal? Star, int Total)>> GetRatingCountsAsync(CancellationToken ct = default)
    {
        return await _context.Products
            .GroupBy(p => p.TotalStarRating)
            .Select(g => new ValueTuple<decimal?, int>(
                g.Key,
                g.Count()
            ))
            .ToListAsync(ct);
    }
    public async Task<List<(string Size, int Total)>> GetSizeCountsAsync(CancellationToken ct = default)
    {
        return await _context.Products
            .GroupBy(p => p.Size)
            .Select(g => new ValueTuple<string, int>(
                g.Key,
                g.Count()
            ))
            .ToListAsync(ct);
    }
}