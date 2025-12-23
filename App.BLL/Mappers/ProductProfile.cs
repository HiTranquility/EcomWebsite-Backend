using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using App.BLL.Dtos.ProductDto.Requests;
using App.BLL.Dtos.ProductDto.Results;
using App.BLL.Dtos.ProductDto.Shares;
using App.DAL.ProductModels;
using AutoMapper;


namespace App.BLL.Mappers;

public class ProductProfile : Profile
{
    private static JsonArray? ParseInformation(string? information)
    {
        if (string.IsNullOrWhiteSpace(information))
            return null;
        
        try
        {
            // Dùng JsonDocument để đọc và tạo JsonArray mới từ đầu
            using var document = JsonDocument.Parse(information);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
                return null;

            // Tạo JsonArray mới và copy các elements bằng cách serialize/deserialize từng element
            var jsonArray = new JsonArray();
            foreach (var element in document.RootElement.EnumerateArray())
            {
                // Serialize element thành string rồi parse lại để tạo node mới không có parent
                var elementJson = element.GetRawText();
                var newNode = JsonNode.Parse(elementJson);
                jsonArray.Add(newNode);
            }
            return jsonArray;
        }
        catch
        {
            return null;
        }
    }

    public ProductProfile()
    {
        // Items
        CreateMap<ProductCategory, ProductCategoryItem>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.Name, o => o.MapFrom(s => s.Title));

        CreateMap<ProductImage, ProductImageItem>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.Url, o => o.MapFrom(s => s.ImageUrl ?? string.Empty));

        // Product -> ProductListRes
        CreateMap<Product, ProductListRes>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.Name, o => o.MapFrom(s => s.Title))
            .ForMember(d => d.Slug, o => o.Ignore()) // Can be generated from Title if needed
            .ForMember(d => d.Description, o => o.MapFrom(s => s.ShortDescription))
            .ForMember(d => d.Price, o => o.MapFrom(s => s.LatestPrice))
            .ForMember(d => d.OriginalPrice, o => o.MapFrom(s => s.OriginalPrice))
            .ForMember(d => d.Currency, o => o.Ignore()) // Can be set to default currency if needed
            .ForMember(d => d.Rating, o => o.MapFrom(s => s.TotalStarRating))
            .ForMember(d => d.ReviewCount, o => o.MapFrom(s => s.ReviewCount))
            .ForMember(d => d.ThumbnailUrl, o => o.MapFrom(s => s.MainImageUrl))
            .ForMember(d => d.ImageUrls, o => o.MapFrom(s => s.ProductImages
                .Where(img => img.DeletedAt == null && !string.IsNullOrEmpty(img.ImageUrl))
                .Select(img => new ProductImageItem
                {
                    Id = img.Id.ToString(),
                    Url = img.ImageUrl ?? string.Empty
                })
                .ToList()))
            .ForMember(d => d.Categories, o => o.MapFrom(s => s.ProductCategory != null
                ? new List<ProductCategoryItem>
                {
                    new ProductCategoryItem
                    {
                        Id = s.ProductCategory.Id.ToString(),
                        Name = s.ProductCategory.Title
                    }
                }
                : new List<ProductCategoryItem>()))
            .ForMember(d => d.Tags, o => o.MapFrom(s => new List<ProductTagItem>())) // No tag relationship, return empty list
            .ForMember(d => d.ManufacturerName, o => o.MapFrom(s => s.Manufacturer != null ? s.Manufacturer.Name : null))
            .ForMember(d => d.ManufacturerId, o => o.MapFrom(s => s.Manufacturer != null ? s.Manufacturer.Id : (int?)null))
            .ForMember(d => d.ManufacturerTotal, o => o.MapFrom(s => s.Manufacturer != null ? s.Manufacturer.Total : (int?)null))
            .ForMember(d => d.IsFreeShipping, o => o.MapFrom(s => s.IsFreeShipping))
            .ForMember(d => d.IsFlashsale, o => o.MapFrom(s => s.IsFlashsale))
            .ForMember(d => d.IsFeature, o => o.MapFrom(s => s.IsFeature))
            .ForMember(d => d.IsSpecial, o => o.MapFrom(s => s.IsSpecial))
            .ForMember(d => d.IsWeekly, o => o.MapFrom(s => s.IsWeekly))
            .ForMember(d => d.IsToday, o => o.MapFrom(s => s.IsToday))
            .ForMember(d => d.IsDeal, o => o.MapFrom(s => s.IsDeal))
            .ForMember(d => d.DiscountPercent, o => o.MapFrom(s =>
                s.OriginalPrice.HasValue && s.LatestPrice.HasValue && s.OriginalPrice.Value > 0m
                    ? Math.Round((1m - s.LatestPrice.Value / s.OriginalPrice.Value) * 100m, 2, MidpointRounding.AwayFromZero)
                    : (decimal?)null));

        // Product -> ProductDetailRes
        CreateMap<Product, ProductInformationRes>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.ToString()))
            .ForMember(d => d.Name, o => o.MapFrom(s => s.Title))
            .ForMember(d => d.Description, o => o.MapFrom(s => s.LongDescription ?? s.ShortDescription))
            .ForMember(d => d.Price, o => o.MapFrom(s => s.LatestPrice))
            .ForMember(d => d.OriginalPrice, o => o.MapFrom(s => s.OriginalPrice))
            .ForMember(d => d.Rating, o => o.MapFrom(s => s.TotalStarRating))
            .ForMember(d => d.ReviewCount, o => o.MapFrom(s => s.ReviewCount))
            .ForMember(d => d.ThumbnailUrl, o => o.MapFrom(s => s.MainImageUrl))
            .ForMember(d => d.ImageUrls, o => o.MapFrom(s => s.ProductImages
                .Where(img => img.DeletedAt == null && !string.IsNullOrEmpty(img.ImageUrl))
                .Select(img => new ProductImageItem
                {
                    Id = img.Id.ToString(),
                    Url = img.ImageUrl ?? string.Empty
                })
                .ToList()))
            .ForMember(d => d.Quantity, o => o.MapFrom(s => s.Quantity))
            .ForMember(d => d.StockLeft, o => o.MapFrom(s => s.StockLeft))
            .ForMember(d => d.Size, o => o.MapFrom(s => s.Size))
            .ForMember(d => d.ManufacturerName, o => o.MapFrom(s => s.Manufacturer != null ? s.Manufacturer.Name : null))
            .ForMember(d => d.ManufacturerId, o => o.MapFrom(s => s.Manufacturer != null ? s.Manufacturer.Id : (int?)null))
            .ForMember(d => d.ManufacturerTotal, o => o.MapFrom(s => s.Manufacturer != null ? s.Manufacturer.Total : (int?)null))
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.ProductCategory != null ? s.ProductCategory.Title : null))
            .ForMember(d => d.IsFreeShipping, o => o.MapFrom(s => s.IsFreeShipping))
            .ForMember(d => d.IsFlashsale, o => o.MapFrom(s => s.IsFlashsale))
            .ForMember(d => d.IsFeature, o => o.MapFrom(s => s.IsFeature))
            .ForMember(d => d.IsSpecial, o => o.MapFrom(s => s.IsSpecial))
            .ForMember(d => d.IsWeekly, o => o.MapFrom(s => s.IsWeekly))
            .ForMember(d => d.IsToday, o => o.MapFrom(s => s.IsToday))
            .ForMember(d => d.IsDeal, o => o.MapFrom(s => s.IsDeal))
            .ForMember(d => d.CreatedAt, o => o.MapFrom(s => s.CreatedAt))
            .ForMember(d => d.Categories, o => o.Ignore())
            .ForMember(d => d.Tags, o => o.Ignore())
            .ForMember(d => d.DiscountPercent, o => o.MapFrom(s =>
                s.OriginalPrice.HasValue && s.LatestPrice.HasValue && s.OriginalPrice.Value > 0m
                    ? Math.Round((1m - s.LatestPrice.Value / s.OriginalPrice.Value) * 100m, 2, MidpointRounding.AwayFromZero)
                    : (decimal?)null));
        
        
        // Product -> ProductDescriptionRes
        CreateMap<Product, ProductDescriptionRes>()
            .ForMember(d => d.LongDescription, o => o.MapFrom(s => s.LongDescription))
            .ForMember(d => d.Information, o => o.Ignore())
            .AfterMap((src, dest) =>
            {
                dest.Information = ParseInformation(src.Information);
            });

        // ProductReview mappings
        CreateMap<CreateReviewReq, ProductReview>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.ProductId, o => o.Ignore())
            .ForMember(d => d.UserId, o => o.Ignore())
            .ForMember(d => d.StarRating, o => o.MapFrom(s => s.StarRating))
            .ForMember(d => d.Content, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.Content) ? null : s.Content.Trim()))
            .ForMember(d => d.FullName, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.FullName) ? null : s.FullName.Trim()))
            .ForMember(d => d.Email, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.Email) ? null : s.Email.Trim()))
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.DeletedAt, o => o.Ignore())
            .ForMember(d => d.Product, o => o.Ignore());

        CreateMap<ProductReview, ProductReviewRes>();
    }
}