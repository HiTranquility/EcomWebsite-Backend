using System;
using System.Collections.Generic;

namespace App.DAL.ProductModels;

public partial class Product
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Slug { get; set; }

    public decimal? TotalStarRating { get; set; }

    public string? ShortDescription { get; set; }

    public string? LongDescription { get; set; }

    public uint? Quantity { get; set; }

    public uint? StockLeft { get; set; }

    public decimal? OriginalPrice { get; set; }

    public decimal? LatestPrice { get; set; }

    public string? MainImageUrl { get; set; }

    public string? EmbeddedUrl { get; set; }

    public string? Size { get; set; }

    public bool? IsFreeShipping { get; set; }

    public bool? IsFlashsale { get; set; }

    public bool? IsFeature { get; set; }

    public bool? IsSpecial { get; set; }

    public bool? IsWeekly { get; set; }

    public bool? IsToday { get; set; }

    public bool? IsDeal { get; set; }

    public int? ReviewCount { get; set; }

    public int? ManufacturerId { get; set; }

    public int? ProductCategoryId { get; set; }

    public string? Information { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Manufacturer? Manufacturer { get; set; }

    public virtual ProductCategory? ProductCategory { get; set; }

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
}
