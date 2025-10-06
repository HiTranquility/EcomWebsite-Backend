using System;
using System.Collections.Generic;

namespace App.DAL.BlogModels;

public partial class BlogCategory
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public int? BlogId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Blog? Blog { get; set; }

    public virtual ICollection<BlogCategoryJoin> BlogCategoryJoins { get; set; } = new List<BlogCategoryJoin>();
}
