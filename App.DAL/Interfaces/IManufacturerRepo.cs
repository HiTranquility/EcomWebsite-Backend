using App.DAL.ProductModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.Interfaces;

public interface IManufacturerRepo : IGenericRepo<Manufacturer>
{
    Task<List<(int? ManufacturerId, string? Name, int Total)>> GetManufacturerCountsAsync(CancellationToken ct = default);
}

