using System.Threading;
using System.Threading.Tasks;
using App.DAL.ProductModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.DataSeedings;

public sealed class ProductSchema : SeedSchema<EcomProductsContext>
{
    public int ManufacturersCount { get; set; }
    public int CategoriesCount { get; set; }
    public int ProductsCount { get; set; }
    public int ImagesPerProduct { get; set; }

    internal EcomProductsContext Db => Context;

    public ProductSchema(EcomProductsContext context) : base(context)
    {
    }

    public override Task RunAsync(CancellationToken ct) 
    => ModelBuilderExtensions.SeedProductTablesAsync(this, ct);
}

