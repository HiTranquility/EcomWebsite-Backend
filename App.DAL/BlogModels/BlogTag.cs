using System;
using System.Collections.Generic;

namespace App.DAL.BlogModels;

public partial class BlogTag
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<BlogTagJoin> BlogTagJoins { get; set; } = new List<BlogTagJoin>();
}
