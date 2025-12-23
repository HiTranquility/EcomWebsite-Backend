using App.DAL.Interfaces;
using App.DAL.ProductModels;
using App.UTIL.Abstractions.DAL;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.Repositories;

public class ManufacturerRepo : GenericRepo<EcomProductsContext, Manufacturer>, IManufacturerRepo
{
    public ManufacturerRepo(EcomProductsContext context) : base(context)
    {
    }
    public async Task<List<(int? ManufacturerId, string? Name, int Total)>> GetManufacturerCountsAsync(CancellationToken ct = default)
    {
        return await _context.Products
            .Include(p => p.Manufacturer)
            .GroupBy(p => new { p.ManufacturerId, p.Manufacturer.Name })
            .Select(g => new ValueTuple<int?, string?, int>(
                g.Key.ManufacturerId,
                g.Key.Name,
                g.Count()
            ))
            .ToListAsync(ct);
    }
}

