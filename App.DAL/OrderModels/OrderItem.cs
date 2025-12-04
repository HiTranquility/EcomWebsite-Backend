using System;
using System.Collections.Generic;

namespace App.DAL.OrderModels;

public partial class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public int? VariantId { get; set; }

    public int Quantity { get; set; }

    public decimal PriceAtTime { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}
