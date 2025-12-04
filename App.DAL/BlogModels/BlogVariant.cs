using System;
using System.Collections.Generic;

namespace App.DAL.BlogModels;

public partial class BlogVariant
{
    public int Id { get; set; }

    public int BlogId { get; set; }

    public string Type { get; set; } = null!;

    public string Url { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Blog Blog { get; set; } = null!;
}
