using System;
using System.Collections.Generic;

namespace App.DAL.BlogModels;

public partial class Quote
{
    public int Id { get; set; }

    public string? Content { get; set; }

    public string? Author { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();
}
