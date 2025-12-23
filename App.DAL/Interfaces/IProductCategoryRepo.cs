using App.DAL.ProductModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.Interfaces;

public interface IProductCategoryRepo : IGenericRepo<ProductCategory>
{
    Task<List<(int CategoryId, int Total)>> GetCategoryCountsAsync(CancellationToken ct = default);
    Task<List<ProductCategory>> GetCategoryTreeWithCountsAsync(CancellationToken ct = default);
}

