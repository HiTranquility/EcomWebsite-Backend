using App.DAL.ProductModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.Interfaces;

public interface IProductRepo : IGenericRepo<Product>
{
    Task<List<(decimal? Star, int Total)>> GetRatingCountsAsync(CancellationToken ct = default);
    Task<List<(string Size, int Total)>> GetSizeCountsAsync(CancellationToken ct = default);
}

