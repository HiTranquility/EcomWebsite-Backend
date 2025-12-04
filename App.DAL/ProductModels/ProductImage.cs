using System;
using System.Collections.Generic;

namespace App.DAL.ProductModels;

public partial class ProductImage
{
    public int Id { get; set; }

    public string? ImageUrl { get; set; }

    public int ProductId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Product Product { get; set; } = null!;
}
