using App.DAL.OrderModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.DataSeedings;

public sealed class OrderSchema : SeedSchema<EcomOrdersContext>
{
    public int OrdersCount { get; set; } = 100;
    public int MaxItemsPerOrder { get; set; } = 5;

    internal EcomOrdersContext Db => Context;

    public OrderSchema(EcomOrdersContext context) : base(context)
    {
    }

    public override Task RunAsync(CancellationToken ct) =>
        ModelBuilderExtensions.SeedOrderTablesAsync(this, ct);
}


