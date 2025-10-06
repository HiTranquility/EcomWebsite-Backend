using System;
using System.Collections.Generic;

namespace App.DAL.BlogModels;

public partial class BlogTagJoin
{
    public int Id { get; set; }

    public int BlogId { get; set; }

    public int BlogTagId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Blog Blog { get; set; } = null!;

    public virtual BlogTag BlogTag { get; set; } = null!;
}
