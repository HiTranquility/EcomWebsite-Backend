using System;
using System.Collections.Generic;

namespace App.DAL.BlogModels;

public partial class BlogComment
{
    public int Id { get; set; }

    public string? Content { get; set; }

    public int? Parent { get; set; }

    public int? BlogId { get; set; }

    public int? UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Blog? Blog { get; set; }

    public virtual ICollection<BlogComment> InverseParentNavigation { get; set; } = new List<BlogComment>();

    public virtual BlogComment? ParentNavigation { get; set; }
}
