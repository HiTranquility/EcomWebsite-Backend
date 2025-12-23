using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using App.UTIL.Abstractions.DTO.Filter;
using Microsoft.AspNetCore.Mvc;

namespace App.BLL.Dtos.ProductDto;

public class ProductFilter : BaseFilter
{
    protected override int MinPageSize => 4;
    protected override int MaxPageSize => 48;
    protected override int DefaultPageSize => 12;

    [StringLength(50, ErrorMessage = "CATEGORY_LENGTH_INVALID|Category must not exceed 50 characters.")]
    [FromQuery(Name = "category")]
    public string? Category { get; set; }
    [StringLength(50, ErrorMessage = "MANUFACTURER_LENGTH_INVALID|Manufacturer must not exceed 50 characters.")]
    [FromQuery(Name = "manufacturer")]
    public string? Manufacturer { get; set; }
    [StringLength(50, ErrorMessage = "PRODUCT_CATEGORY_LENGTH_INVALID|ProductCategory must not exceed 50 characters.")]
    [FromQuery(Name = "productCategory")]
    public string? ProductCategory { get; set; }
    [StringLength(50, ErrorMessage = "PRODUCT_TAG_LENGTH_INVALID|ProductTag must not exceed 50 characters.")]
    [FromQuery(Name = "productTag")]
    public string? ProductTag { get; set; }

    [FromQuery(Name = "sort")]
    public string? Sort { get; set; }

    [Range(0, 1000000, ErrorMessage = "PRICE_MIN_INVALID|Minimum price must be positive.")]
    [FromQuery(Name = "priceMin")]
    public decimal? PriceMin { get; set; }

    [Range(0, 1000000, ErrorMessage = "PRICE_MAX_INVALID|Maximum price must be positive.")]
    [FromQuery(Name = "priceMax")]
    public decimal? PriceMax { get; set; }

    [Range(1, 5, ErrorMessage = "RATING_MIN_INVALID|Rating must be between 1 and 5.")]
    [FromQuery(Name = "minRating")]
    public decimal? MinRating { get; set; }

    [FromQuery(Name = "isFreeShipping")]
    public bool? IsFreeShipping { get; set; }

    [FromQuery(Name = "sizes")]
    public List<string>? Sizes { get; set; }

    [FromQuery(Name = "isFlashsale")]
    public bool? IsFlashsale { get; set; }

    [FromQuery(Name = "isFeature")]
    public bool? IsFeature { get; set; }

    [FromQuery(Name = "isSpecial")]
    public bool? IsSpecial { get; set; }

    [FromQuery(Name = "isWeekly")]
    public bool? IsWeekly { get; set; }

    [FromQuery(Name = "isToday")]
    public bool? IsToday { get; set; }

    [FromQuery(Name = "isDeal")]
    public bool? IsDeal { get; set; }

    public override void Normalize()
    {
        base.Normalize();

        if (string.IsNullOrWhiteSpace(Sort))
        {
            Sort = null;
        }
        else
        {
            Sort = Sort.Trim().ToLowerInvariant();
        }
    }
}