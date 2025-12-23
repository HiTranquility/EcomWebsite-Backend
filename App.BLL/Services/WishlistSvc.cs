using App.BLL.Dtos.WishlistDto.Results;
using App.DAL.ProductModels;
using App.DAL.Repositories;
using App.DAL.UserModels;
using App.UTIL.Abstractions.BLL;
using App.UTIL.Abstractions.DTO.Response;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

using App.BLL.Interfaces;

namespace App.BLL.Services;

public class WishlistSvc : GenericSvc<WishlistRepo, Wishlist>, IWishlistSvc
{
    private readonly ProductRepo _productRepo;

    public WishlistSvc(WishlistRepo wishlistRepo, ProductRepo productRepo, IMapper mapper) : base(wishlistRepo, mapper)
    {
        _productRepo = productRepo;
    }

    public async Task<BaseResponse> GetItemsAsync(int userId, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        var items = await _repo.All
            .AsNoTracking()
            .TagWith("WishlistSvc.GetWishlistAsync")
            .Where(w => w.UserId == userId && w.DeletedAt == null)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(ct);

        if (items.Count == 0)
        {
            rsp.SetData(new List<WishlistItemRes>(), "Wishlist is empty", 200);
            return rsp;
        }

        var productIds = items
            .Select(w => w.ProductId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var products = await _productRepo.All
            .AsNoTracking()
            .TagWith("WishlistSvc.GetWishlistAsync.GetProducts")
            .Where(p => productIds.Contains(p.Id) && p.DeletedAt == null)
            .ToDictionaryAsync(p => p.Id, ct);

        var result = items
            .Where(w => w.ProductId.HasValue)
            .Select(w =>
            {
                var pid = w.ProductId!.Value;
                products.TryGetValue(pid, out var p);

                return new WishlistItemRes
                {
                    Id = w.Id,
                    ProductId = pid,
                    Name = p?.Title ?? $"Product #{pid}",
                    Slug = p?.Slug,
                    Price = p?.LatestPrice ?? p?.OriginalPrice,
                    ThumbnailUrl = p?.MainImageUrl,
                    CreatedAt = w.CreatedAt
                };
            })
            .ToList();

        rsp.SetData(result, "Get wishlist successfully", 200);
        return rsp;
    }

    public async Task<BaseResponse> AddItemAsync(int userId, int productId, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        // Validation
        if (productId <= 0)
        {
            rsp.SetError("PRODUCT_ID_INVALID", "Invalid Product ID", "ProductId must be greater than 0", 400);
            return rsp;
        }

        // Check if product exists
        var productExists = await _productRepo.All
            .AsNoTracking()
            .TagWith("WishlistSvc.AddAsync.CheckProduct")
            .AnyAsync(p => p.Id == productId && p.DeletedAt == null, ct);

        if (!productExists)
        {
            rsp.SetError("PRODUCT_NOT_FOUND", "Product Not Found", "The product you are trying to add does not exist", 404);
            return rsp;
        }

        // Check if already in wishlist
        var exists = await _repo.All
            .AsNoTracking()
            .TagWith("WishlistSvc.AddAsync.CheckExists")
            .AnyAsync(w => w.UserId == userId && w.ProductId == productId && w.DeletedAt == null, ct);

        if (exists)
        {
            rsp.SetData(null, "Item already in wishlist", 200);
            return rsp;
        }

        var entity = new Wishlist
        {
            UserId = userId,
            ProductId = productId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repo.CreateAsync(entity, ct);
        rsp.SetData(null, "Item added to wishlist successfully", 201);
        return rsp;
    }

    public async Task<BaseResponse> RemoveItemAsync(int userId, int id, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        var entity = await _repo.All
            .TagWith("WishlistSvc.RemoveAsync")
            .FirstOrDefaultAsync(w => w.Id == id && w.UserId == userId && w.DeletedAt == null, ct);

        if (entity == null)
        {
            rsp.SetError("WISHLIST_NOT_FOUND", "Wishlist item not found", "Wishlist item not found", 404);
            return rsp;
        }

        entity.DeletedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(entity, ct);
        rsp.SetData(null, "Item removed from wishlist successfully", 200);
        return rsp;
    }

    public async Task<BaseResponse> ClearAllAsync(int userId, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        var items = await _repo.All
            .TagWith("WishlistSvc.ClearAsync")
            .Where(w => w.UserId == userId && w.DeletedAt == null)
            .ToListAsync(ct);

        if (items.Count == 0)
        {
            rsp.SetData(null, "Wishlist is already empty", 200);
            return rsp;
        }

        var now = DateTime.UtcNow;
        foreach (var item in items)
        {
            item.DeletedAt = now;
        }

        await _repo.UpdateAsync(items, ct);
        rsp.SetData(null, "Wishlist cleared successfully", 200);
        return rsp;
    }
}


