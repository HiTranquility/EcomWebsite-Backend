using App.BLL.Dtos.WishlistDto.Results;
using App.DAL.ProductModels;
using App.DAL.Repositories;
using App.DAL.UserModels;
using App.UTIL.Abstractions.BLL;
using App.UTIL.Abstractions.DTO.Response;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace App.BLL.Services;

public class WishlistSvc : GenericSvc<WishlistRepo, Wishlist>
{
    private readonly ProductRepo _productRepo;

    public WishlistSvc(WishlistRepo wishlistRepo, ProductRepo productRepo, IMapper mapper) : base(wishlistRepo, mapper)
    {
        _productRepo = productRepo;
    }

    public async Task<BaseResponse> GetWishlistAsync(int userId, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        List<Wishlist> items = await _repo.All
            .AsNoTracking()
            .Where(w => w.UserId == userId && w.DeletedAt == null)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(ct);

        var productIds = items
            .Select(w => w.ProductId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        Dictionary<int, Product> products = await _productRepo.All
            .AsNoTracking()
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
                    Price = p?.LastestPrice ?? p?.OriginalPrice,
                    ThumbnailUrl = p?.MainImageUrl,
                    CreatedAt = w.CreatedAt
                };
            })
            .ToList();

        rsp.SetData(result);
        return rsp;
    }

    public async Task<BaseResponse> AddAsync(int userId, int productId, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        bool exists = await _repo.All
            .AsNoTracking()
            .AnyAsync(w => w.UserId == userId && w.ProductId == productId && w.DeletedAt == null, ct);

        if (exists)
        {
            rsp.SetData(null);
            return rsp;
        }

        var now = DateTime.UtcNow;
        var entity = new Wishlist
        {
            UserId = userId,
            ProductId = productId,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repo.CreateAsync(entity, ct);
        rsp.SetData(null, "Created", 201);
        return rsp;
    }

    public async Task<BaseResponse> RemoveAsync(int userId, int id, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        Wishlist? entity = await _repo.All
            .Where(w => w.Id == id && w.UserId == userId && w.DeletedAt == null)
            .FirstOrDefaultAsync(ct);

        if (entity == null)
        {
            rsp.SetError("WISHLIST_NOT_FOUND", "Wishlist item not found", "Wishlist item not found", 404);
            return rsp;
        }

        entity.DeletedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(entity, ct);
        rsp.SetData(null);
        return rsp;
    }

    public async Task<BaseResponse> ClearAsync(int userId, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        List<Wishlist> items = await _repo.All
            .Where(w => w.UserId == userId && w.DeletedAt == null)
            .ToListAsync(ct);

        if (items.Count == 0)
        {
            rsp.SetData(null);
            return rsp;
        }

        var now = DateTime.UtcNow;
        foreach (var w in items)
        {
            w.DeletedAt = now;
        }

        await _repo.UpdateAsync(items, ct);
        rsp.SetData(null);
        return rsp;
    }
}


