using System;
using System.Collections.Generic;

namespace App.DAL.BlogModels;

public partial class BlogCategoryJoin
{
    public int Id { get; set; }

    public int BlogId { get; set; }

    public int BlogCategoryId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Blog Blog { get; set; } = null!;

    public virtual BlogCategory BlogCategory { get; set; } = null!;
}
