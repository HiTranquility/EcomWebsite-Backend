using System;
using System.Collections.Generic;

namespace App.DAL.ProductModels;

public partial class ProductCategory
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public int? Parent { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<ProductCategory> InverseParentNavigation { get; set; } = new List<ProductCategory>();

    public virtual ProductCategory? ParentNavigation { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
