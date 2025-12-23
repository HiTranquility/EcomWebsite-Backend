using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using App.BLL.Dtos.ProductDto;
using App.BLL.Dtos.ProductDto.Requests;
using App.BLL.Dtos.ProductDto.Results;
using App.BLL.Dtos.ProductDto.Shares;
using App.DAL.ProductModels;
using App.DAL.Repositories;
using App.UTIL.Abstractions.BLL;
using App.UTIL.Abstractions.DTO.Response;
using App.INFRA.Caching;
using App.UTIL.Helpers.Cache.Schemas;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using LinqKit;

using App.BLL.Interfaces;

namespace App.BLL.Services;

public class ProductSvc : GenericSvc<ProductRepo, Product>, IProductSvc
{

    private readonly ICacheService _cacheService;
    private readonly ProductReviewRepo _productReviewRepo;
    private readonly ManufacturerRepo _manufacturerRepo;
    private readonly ProductCategoryRepo _productCategoryRepo;

    public ProductSvc(
        ProductRepo repo,
        ProductReviewRepo productReviewRepo,
        ManufacturerRepo manufacturerRepo,
        ProductCategoryRepo productCategoryRepo,
        IMapper mapper,
        ICacheService cacheService) : base(repo, mapper)
    {
        _cacheService = cacheService;
        _productReviewRepo = productReviewRepo;
        _manufacturerRepo = manufacturerRepo;
        _productCategoryRepo = productCategoryRepo;
    }
    public async Task<BaseResponse> GetProductListAsync(ProductFilter filter, CancellationToken ct = default) 
    {
        var rsp = new BaseResponse();
        filter.Normalize();
        var cachePrefix = ProductCacheSchema.ListPrefix;
        var cacheKey = ProductCacheSchema.BuildListKey(
            filter.Keyword,
            filter.Category,
            filter.Manufacturer,
            filter.ProductCategory,
            filter.ProductTag,
            filter.Sort,
            filter.PriceMin,
            filter.PriceMax,
            filter.MinRating,
            filter.IsFreeShipping,
            filter.IsFlashsale,
            filter.IsFeature,
            filter.IsSpecial,
            filter.IsWeekly,
            filter.IsToday,
            filter.IsDeal,
            filter.Sizes != null ? string.Join(",", filter.Sizes) : null,
            filter.Page,
            filter.PageSize);

        //Get Data From Cache
        var (total, items) = await _cacheService.GetOrSetAsync(cacheKey, async token =>
        {
            var query = _repo.All
                .AsNoTracking()
                .AsExpandable()
                .TagWith("ProductSvc.GetProductListAsync");

            // KEYWORD
            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var kw = filter.Keyword.Trim();
                query = query.Where(p =>
                    EF.Functions.Like(p.Title ?? string.Empty, $"%{kw}%") ||
                    EF.Functions.Like(p.ShortDescription ?? string.Empty, $"%{kw}%") ||
                    EF.Functions.Like(p.LongDescription ?? string.Empty, $"%{kw}%"));
            }

            // CATEGORY
            if (!string.IsNullOrWhiteSpace(filter.Category))
            {
                var category = filter.Category.Trim();
                query = query.Where(p =>
                    p.ProductCategory != null && p.ProductCategory.Title == category);
            }

            // MANUFACTURER
            if (!string.IsNullOrWhiteSpace(filter.Manufacturer))
            {
                var manufacturer = filter.Manufacturer.Trim();
                query = query.Where(p =>
                    p.Manufacturer != null && p.Manufacturer.Name == manufacturer);
            }

            // PRODUCT CATEGORY
            if (!string.IsNullOrWhiteSpace(filter.ProductCategory))
            {
                var productCategory = filter.ProductCategory.Trim();
                query = query.Where(p =>
                    p.ProductCategory != null && p.ProductCategory.Title == productCategory);
            }

            // PRODUCT TAG (if stored in Information field or other field)
            if (!string.IsNullOrWhiteSpace(filter.ProductTag))
            {
                var productTag = filter.ProductTag.Trim();
                query = query.Where(p =>
                    p.Information != null && EF.Functions.Like(p.Information, $"%{productTag}%"));
            }

            // PRICE RANGE
            if (filter.PriceMin.HasValue)
            {
                query = query.Where(p => p.LatestPrice.HasValue && p.LatestPrice >= filter.PriceMin.Value);
            }

            if (filter.PriceMax.HasValue)
            {
                query = query.Where(p => p.LatestPrice.HasValue && p.LatestPrice <= filter.PriceMax.Value);
            }

            // MIN RATING
            if (filter.MinRating.HasValue)
            {
                query = query.Where(p => p.TotalStarRating.HasValue && p.TotalStarRating >= filter.MinRating.Value);
            }

            // IS FREE SHIPPING
            if (filter.IsFreeShipping.HasValue)
            {
                query = query.Where(p => p.IsFreeShipping == filter.IsFreeShipping.Value);
            }

            // SIZES
            if (filter.Sizes is { Count: > 0 })
            {
                var sizeSet = filter.Sizes
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (sizeSet.Count > 0)
                {
                    var predicate = PredicateBuilder.New<Product>(false);
                    foreach (var size in sizeSet)
                    {
                        var sizeValue = size;
                        predicate = predicate.Or(p => p.Size != null && EF.Functions.Like(p.Size, $"%{sizeValue}%"));
                    }
                    query = query.Where(predicate);
                }
            }

            // BOOLEAN FLAGS
            if (filter.IsFlashsale.HasValue)
            {
                query = query.Where(p => p.IsFlashsale == filter.IsFlashsale.Value);
            }

            if (filter.IsFeature.HasValue)
            {
                query = query.Where(p => p.IsFeature == filter.IsFeature.Value);
            }

            if (filter.IsSpecial.HasValue)
            {
                query = query.Where(p => p.IsSpecial == filter.IsSpecial.Value);
            }

            if (filter.IsWeekly.HasValue)
            {
                query = query.Where(p => p.IsWeekly == filter.IsWeekly.Value);
            }

            if (filter.IsToday.HasValue)
            {
                query = query.Where(p => p.IsToday == filter.IsToday.Value);
            }

            if (filter.IsDeal.HasValue)
            {
                query = query.Where(p => p.IsDeal == filter.IsDeal.Value);
            }

            var total = await query.CountAsync(token);

            // Order by CreatedAt descending (newest first)
            var orderedQuery = query.OrderByDescending(p => p.CreatedAt);

            var projectedItems = await orderedQuery
                .AsSplitQuery()
                .ProjectTo<ProductListRes>(_mapper.ConfigurationProvider)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(token);

            return (total, projectedItems);
        }, ttl: ProductCacheSchema.ListTtl, prefix: cachePrefix, cancellationToken: ct);

        rsp.SetData(new
        {
            Total = (int)total,
            Items = items,
            Page = filter.Page,
            PageSize = filter.PageSize
        });
        return rsp;
    }

    public async Task<BaseResponse> GetProductByIdAsync(int id, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();
        var cachePrefix = ProductCacheSchema.DetailPrefix;
        var cacheKey = ProductCacheSchema.BuildDetailKeyById(id);
        var product = await _cacheService.GetOrSetAsync(cacheKey, async token =>
        {
            var products = await _repo.All
                .AsNoTracking()
                .Include(p => p.Manufacturer)
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductImages)
                .TagWith("ProductSvc.GetProductDetailAsync")
                // 🔹 Lấy luôn 3 sản phẩm gần nhau (hiện tại, trước, sau)
                .Where(p => p.Id >= id - 1 && p.Id <= id + 1)
                .OrderBy(p => p.Id)
                .ToListAsync(token);
            var detail = products.FirstOrDefault(p => p.Id == id);
            if (detail == null)
                return null;

            // Load category with parent chain if exists
            if (detail.ProductCategoryId.HasValue)
            {
                var category = await _productCategoryRepo.All
                    .AsNoTracking()
                    .Include(c => c.ParentNavigation)
                    .Where(c => c.Id == detail.ProductCategoryId.Value)
                    .FirstOrDefaultAsync(token);

                if (category != null && category.ParentNavigation != null)
                {
                    // Load parent's parent if exists
                    var parentId = category.ParentNavigation.Id;
                    var parent = await _productCategoryRepo.All
                        .AsNoTracking()
                        .Include(c => c.ParentNavigation)
                        .Where(c => c.Id == parentId)
                        .FirstOrDefaultAsync(token);
                    
                    if (parent != null)
                    {
                        category.ParentNavigation = parent;
                    }
                }

                if (category != null)
                {
                    detail.ProductCategory = category;
                }
            }

            return _mapper.Map<ProductInformationRes>(detail, options =>
            {
                options.AfterMap((src, dest) =>
                {
                    var product = src as Product;
                    if (product == null) return;

                    // Map Categories từ ProductCategory
                    if (product.ProductCategory != null)
                    {
                        dest.Categories = new List<ProductCategoryItem>
                        {
                            new ProductCategoryItem
                            {
                                Id = product.ProductCategory.Id.ToString(),
                                Name = product.ProductCategory.Title
                            }
                        };

                        // Build breadcrumb path from root to current category
                        var pathStack = new Stack<ProductCategoryItem>();
                        var currentCategory = product.ProductCategory;

                        // Traverse up the parent chain
                        while (currentCategory != null)
                        {
                            pathStack.Push(new ProductCategoryItem
                            {
                                Id = currentCategory.Id.ToString(),
                                Name = currentCategory.Title ?? string.Empty
                            });
                            currentCategory = currentCategory.ParentNavigation;
                        }

                        // Convert stack to list (from root to current)
                        dest.BreadcrumbPath = pathStack.ToList();
                    }

                    // Tags có thể lấy từ Information field hoặc để empty
                    dest.Tags = new List<ProductTagItem>();
                });
            });

        }, ttl: ProductCacheSchema.DetailTtl, prefix: cachePrefix, cancellationToken: ct);
        if (product == null)
        {
            rsp.SetError("PRODUCT_NOT_FOUND", "Product not found", "Product not found", 404);
            return rsp;
        }

        rsp.SetData(product, "Get product detail successfully", 200);
        return rsp;
    }

    public async Task<BaseResponse> GetProductBySlugAsync(string slug, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();
        if (string.IsNullOrWhiteSpace(slug))
        {
            rsp.SetError("INVALID_SLUG", "Slug is required", "Slug is required", 400);
            return rsp;
        }

        var cachePrefix = ProductCacheSchema.DetailPrefix;
        var cacheKey = ProductCacheSchema.BuildDetailKeyBySlug(slug); // Use slug in cache key
        var product = await _cacheService.GetOrSetAsync(cacheKey, async token =>
        {
            var detail = await _repo.All
                .AsNoTracking()
                .Include(p => p.Manufacturer)
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductImages)
                .TagWith("ProductSvc.GetProductInformationBySlugAsync")
                .Where(p => p.Slug != null && p.Slug == slug && p.DeletedAt == null)
                .FirstOrDefaultAsync(token);

            if (detail == null)
                return null;

            // Load category with parent chain if exists
            if (detail.ProductCategoryId.HasValue)
            {
                var category = await _productCategoryRepo.All
                    .AsNoTracking()
                    .Include(c => c.ParentNavigation)
                    .Where(c => c.Id == detail.ProductCategoryId.Value)
                    .FirstOrDefaultAsync(token);

                if (category != null && category.ParentNavigation != null)
                {
                    // Load parent's parent if exists
                    var parentId = category.ParentNavigation.Id;
                    var parent = await _productCategoryRepo.All
                        .AsNoTracking()
                        .Include(c => c.ParentNavigation)
                        .Where(c => c.Id == parentId)
                        .FirstOrDefaultAsync(token);
                    
                    if (parent != null)
                    {
                        category.ParentNavigation = parent;
                    }
                }

                if (category != null)
                {
                    detail.ProductCategory = category;
                }
            }

            return _mapper.Map<ProductInformationRes>(detail, options =>
            {
                options.AfterMap((src, dest) =>
                {
                    var product = src as Product;
                    if (product == null) return;

                    // Map Categories từ ProductCategory
                    if (product.ProductCategory != null)
                    {
                        dest.Categories = new List<ProductCategoryItem>
                        {
                            new ProductCategoryItem
                            {
                                Id = product.ProductCategory.Id.ToString(),
                                Name = product.ProductCategory.Title
                            }
                        };

                        // Build breadcrumb path from root to current category
                        var pathStack = new Stack<ProductCategoryItem>();
                        var currentCategory = product.ProductCategory;

                        // Traverse up the parent chain
                        while (currentCategory != null)
                        {
                            pathStack.Push(new ProductCategoryItem
                            {
                                Id = currentCategory.Id.ToString(),
                                Name = currentCategory.Title ?? string.Empty
                            });
                            currentCategory = currentCategory.ParentNavigation;
                        }

                        // Convert stack to list (from root to current)
                        dest.BreadcrumbPath = pathStack.ToList();
                    }

                    // Tags có thể lấy từ Information field hoặc để empty
                    dest.Tags = new List<ProductTagItem>();
                });
            });

        }, ttl: ProductCacheSchema.DetailTtl, prefix: cachePrefix, cancellationToken: ct);

        if (product == null)
        {
            rsp.SetError("PRODUCT_NOT_FOUND", "Product not found", "Product not found", 404);
            return rsp;
        }

        rsp.SetData(product, "Get product detail successfully", 200);
        return rsp;
    }

    /// <summary>
    /// Batch get products by IDs - reduces N+1 queries to a single query
    /// Used by cart, wishlist, and other pages that need to fetch multiple products
    /// </summary>
    public async Task<BaseResponse> GetProductsByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();
        var idList = ids?.Distinct().ToList() ?? new List<int>();
        
        if (idList.Count == 0)
        {
            rsp.SetData(new List<ProductListRes>(), "No IDs provided", 200);
            return rsp;
        }

        // Limit to prevent abuse
        const int MaxBatchSize = 50;
        if (idList.Count > MaxBatchSize)
        {
            rsp.SetError("TOO_MANY_IDS", $"Maximum {MaxBatchSize} IDs allowed per request", "Too many IDs", 400);
            return rsp;
        }

        var cachePrefix = ProductCacheSchema.ListPrefix;
        var cacheKey = $"{ProductCacheSchema.ListPrefix}:batch:{string.Join(",", idList.OrderBy(id => id))}";
        
        var products = await _cacheService.GetOrSetAsync(cacheKey, async token =>
        {
            var items = await _repo.All
                .AsNoTracking()
                .Include(p => p.Manufacturer)
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductImages)
                .Where(p => idList.Contains(p.Id) && p.DeletedAt == null)
                .TagWith("ProductSvc.GetProductsByIdsAsync")
                .ProjectTo<ProductListRes>(_mapper.ConfigurationProvider)
                .ToListAsync(token);
            
            return items;
        }, ttl: ProductCacheSchema.DetailTtl, prefix: cachePrefix, cancellationToken: ct);

        rsp.SetData(products, "Get products by IDs successfully", 200);
        return rsp;
    }

    public async Task<BaseResponse> GetFiltersAsync(CancellationToken ct = default)
    {
        var rsp = new BaseResponse();
        var cachePrefix = ProductCacheSchema.FilterPrefix;
        var cacheKey = ProductCacheSchema.BuildFilterKey();
        var filter = await _cacheService.GetOrSetAsync(cacheKey, async token =>
        {
            // Get Manufacturers with counts
            var manufacturerCounts = await _manufacturerRepo.GetManufacturerCountsAsync(token);
            var manufacturers = manufacturerCounts
                .Where(m => m.ManufacturerId.HasValue && !string.IsNullOrWhiteSpace(m.Name))
                .Select(m => new ProductFilterManufacturerItem
                {
                    Id = m.ManufacturerId,
                    Name = m.Name ?? string.Empty,
                    Total = m.Total
                })
                .OrderBy(m => m.Name)
                .ToList();

            // Get Categories with counts (tree structure using InverseParentNavigation)
            var categoryCounts = await _productCategoryRepo.GetCategoryCountsAsync(token);
            var categoryCountsDict = categoryCounts.ToDictionary(cc => cc.CategoryId, cc => cc.Total);
            
            // Load all categories with InverseParentNavigation
            var allCategories = await _productCategoryRepo.All
                .AsNoTracking()
                .Where(c => c.DeletedAt == null)
                .Include(c => c.InverseParentNavigation.Where(child => child.DeletedAt == null))
                .ToListAsync(token);
            
            // Build tree: only root categories (Parent == null)
            var categories = allCategories
                .Where(c => c.Parent == null)
                .Select(cat =>
                {
                    var count = categoryCountsDict.GetValueOrDefault(cat.Id, 0);
                    
                    // Recursively build children from InverseParentNavigation
                    var children = cat.InverseParentNavigation
                        .OrderBy(child => child.Title)
                        .Select(child =>
                        {
                            var childCount = categoryCountsDict.GetValueOrDefault(child.Id, 0);
                            
                            // Build grandchildren if any
                            var grandchildren = child.InverseParentNavigation
                                .OrderBy(gc => gc.Title)
                                .Select(gc =>
                                {
                                    var gcCount = categoryCountsDict.GetValueOrDefault(gc.Id, 0);
                                    return new ProductFilterCategoryRes
                                    {
                                        Id = gc.Id,
                                        Title = gc.Title ?? string.Empty,
                                        Total = gcCount,
                                        Children = new List<ProductFilterCategoryRes>()
                                    };
                                })
                                .ToList();
                            
                            return new ProductFilterCategoryRes
                            {
                                Id = child.Id,
                                Title = child.Title ?? string.Empty,
                                Total = childCount,
                                Children = grandchildren
                            };
                        })
                        .ToList();

                    return new ProductFilterCategoryRes
                    {
                        Id = cat.Id,
                        Title = cat.Title ?? string.Empty,
                        Total = count,
                        Children = children
                    };
                })
                .OrderBy(c => c.Title)
                .ToList();

            // Get Ratings with counts
            var ratingCounts = await _repo.GetRatingCountsAsync(token);
            var ratings = ratingCounts
                .Where(r => r.Star.HasValue)
                .Select(r => new ProductFilterRatingItem
                {
                    Star = (int)Math.Round(r.Star!.Value),
                    Total = r.Total
                })
                .OrderByDescending(r => r.Star)
                .ToList();

            // Get Sizes with counts
            var sizeCounts = await _repo.GetSizeCountsAsync(token);
            var sizes = sizeCounts
                .Where(s => !string.IsNullOrWhiteSpace(s.Size))
                .Select(s => new ProductFilterSizeItem
                {
                    Label = s.Size ?? string.Empty,
                    Total = s.Total
                })
                .OrderBy(s => s.Label)
                .ToList();

            return new ProductFilterRes
            {
                Manufacturers = manufacturers,
                Categories = categories,
                Ratings = ratings,
                Sizes = sizes
            };
        }, ttl: ProductCacheSchema.FilterTtl, prefix: cachePrefix, cancellationToken: ct);
        
        rsp.SetData(filter, "Get product filter successfully", 200);
        return rsp;
    }

    public async Task<BaseResponse> GetRelatedProductsAsync(int productId, int limit = 6, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();
        
        // Get current product to find related products
        var currentProduct = await _repo.All
            .AsNoTracking()
            .Where(p => p.Id == productId && p.DeletedAt == null)
            .FirstOrDefaultAsync(ct);

        if (currentProduct == null)
        {
            rsp.SetError("PRODUCT_NOT_FOUND", "Product not found", "Product not found", 404);
            return rsp;
        }

        var cachePrefix = ProductCacheSchema.ListPrefix;
        var cacheKey = $"{ProductCacheSchema.ListPrefix}:related:{productId}:limit:{limit}";
        
        var products = await _cacheService.GetOrSetAsync(cacheKey, async token =>
        {
            var baseQuery = _repo.All
                .AsNoTracking()
                .AsExpandable()
                .Include(p => p.Manufacturer)
                .Include(p => p.ProductCategory)
                .Include(p => p.ProductImages)
                .Where(p => p.Id != productId && p.DeletedAt == null)
                .TagWith("ProductSvc.GetRelatedProductsAsync");

            // Priority: Same category > Same manufacturer > Others
            var sameCategoryQuery = baseQuery
                .Where(p => currentProduct.ProductCategoryId.HasValue && 
                           p.ProductCategoryId == currentProduct.ProductCategoryId)
                .OrderByDescending(p => p.CreatedAt);

            var sameManufacturerQuery = baseQuery
                .Where(p => currentProduct.ManufacturerId.HasValue && 
                           p.ManufacturerId == currentProduct.ManufacturerId &&
                           (!currentProduct.ProductCategoryId.HasValue || p.ProductCategoryId != currentProduct.ProductCategoryId))
                .OrderByDescending(p => p.CreatedAt);

            var otherProductsQuery = baseQuery
                .Where(p => (!currentProduct.ProductCategoryId.HasValue || p.ProductCategoryId != currentProduct.ProductCategoryId) &&
                           (!currentProduct.ManufacturerId.HasValue || p.ManufacturerId != currentProduct.ManufacturerId))
                .OrderByDescending(p => p.CreatedAt);

            var results = new List<ProductListRes>();

            // Get products from same category first
            var sameCategoryProducts = await sameCategoryQuery
                .AsSplitQuery()
                .ProjectTo<ProductListRes>(_mapper.ConfigurationProvider)
                .Take(limit)
                .ToListAsync(token);
            results.AddRange(sameCategoryProducts);

            // If not enough, get from same manufacturer
            if (results.Count < limit)
            {
                var sameManufacturerProducts = await sameManufacturerQuery
                    .AsSplitQuery()
                    .ProjectTo<ProductListRes>(_mapper.ConfigurationProvider)
                    .Take(limit - results.Count)
                    .ToListAsync(token);
                results.AddRange(sameManufacturerProducts);
            }

            // If still not enough, get others
            if (results.Count < limit)
            {
                var otherProducts = await otherProductsQuery
                    .AsSplitQuery()
                    .ProjectTo<ProductListRes>(_mapper.ConfigurationProvider)
                    .Take(limit - results.Count)
                    .ToListAsync(token);
                results.AddRange(otherProducts);
            }

            return results.Take(limit).ToList();
        }, ttl: ProductCacheSchema.ListTtl, prefix: cachePrefix, cancellationToken: ct);

        rsp.SetData(products, "Get related products successfully", 200);
        return rsp;
    }

    public async Task<BaseResponse> GetDescriptionByIdAsync(int id, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();
        var cachePrefix = ProductCacheSchema.DetailPrefix;
        var cacheKey = ProductCacheSchema.BuildDetailKeyById(id) + ":description";
        var description = await _cacheService.GetOrSetAsync(cacheKey, async token =>
        {
            var product = await ReadAsync(id, token);
            if (product == null)
                return null;

            return _mapper.Map<ProductDescriptionRes>(product);
        }, ttl: ProductCacheSchema.DetailTtl, prefix: cachePrefix, cancellationToken: ct);

        if (description == null)
        {
            rsp.SetError("PRODUCT_NOT_FOUND", "Product not found", "Product not found", 404);
            return rsp;
        }

        rsp.SetData(description, "Get product description successfully", 200);
        return rsp;
    }

    public async Task<BaseResponse> GetDescriptionBySlugAsync(string slug, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();
        if (string.IsNullOrWhiteSpace(slug))
        {
            rsp.SetError("INVALID_SLUG", "Slug is required", "Slug is required", 400);
            return rsp;
        }

        var cachePrefix = ProductCacheSchema.DetailPrefix;
        var cacheKey = ProductCacheSchema.BuildDetailKeyBySlug(slug) + ":description";
        var description = await _cacheService.GetOrSetAsync(cacheKey, async token =>
        {
            var product = await ReadAsync(slug, token);
            if (product == null)
                return null;

            return _mapper.Map<ProductDescriptionRes>(product);
        }, ttl: ProductCacheSchema.DetailTtl, prefix: cachePrefix, cancellationToken: ct);

        if (description == null)
        {
            rsp.SetError("PRODUCT_NOT_FOUND", "Product not found", "Product not found", 404);
            return rsp;
        }

        rsp.SetData(description, "Get product description successfully", 200);
        return rsp;
    }
}