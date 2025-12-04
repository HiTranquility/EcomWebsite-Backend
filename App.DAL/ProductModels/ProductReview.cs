using System;
using System.Collections.Generic;

namespace App.DAL.ProductModels;

public partial class ProductReview
{
    public int Id { get; set; }

    public uint? StarRating { get; set; }

    public string? Content { get; set; }

    public string Email { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public int ProductId { get; set; }

    public int? UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Product Product { get; set; } = null!;
}
