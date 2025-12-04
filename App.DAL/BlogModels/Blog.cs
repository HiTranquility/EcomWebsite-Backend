using System;
using System.Collections.Generic;

namespace App.DAL.BlogModels;

public partial class Blog
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Slug { get; set; }

    public string? Content { get; set; }

    public string? Author { get; set; }

    public int? CommentCount { get; set; }

    public int? QuoteId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<BlogCategory> BlogCategories { get; set; } = new List<BlogCategory>();

    public virtual ICollection<BlogCategoryJoin> BlogCategoryJoins { get; set; } = new List<BlogCategoryJoin>();

    public virtual ICollection<BlogComment> BlogComments { get; set; } = new List<BlogComment>();

    public virtual ICollection<BlogTagJoin> BlogTagJoins { get; set; } = new List<BlogTagJoin>();

    public virtual ICollection<BlogVariant> BlogVariants { get; set; } = new List<BlogVariant>();

    public virtual Quote? Quote { get; set; }
}
