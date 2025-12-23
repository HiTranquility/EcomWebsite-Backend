using App.BLL.Dtos.ProductDto.Requests;
using App.BLL.Dtos.ProductDto.Results;
using App.BLL.Dtos.ProductDto.Shares;
using App.DAL.ProductModels;
using App.DAL.Repositories;
using App.DAL.UserModels;
using App.UTIL.Abstractions.BLL;
using App.UTIL.Abstractions.DTO.Response;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

using App.BLL.Interfaces;

namespace App.BLL.Services;

public class ProductTagSvc : IProductTagSvc
{
    private readonly UserTagRepo _userTagRepo;
    private readonly ProductRepo _productRepo;

    public ProductTagSvc(UserTagRepo userTagRepo, ProductRepo productRepo)
    {
        _userTagRepo = userTagRepo;
        _productRepo = productRepo;
    }

    public async Task<BaseResponse> GetProductTagsAsync(int? userId, int productId, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        // Check if product exists
        var product = await _productRepo.ReadAsync(productId, ct);
        if (product == null)
        {
            rsp.SetError("PRODUCT_NOT_FOUND", "Product not found", "Product not found", 404);
            return rsp;
        }

        // If userId is provided, get only that user's tags, otherwise return empty list
        List<UserTag> tags;
        if (userId.HasValue)
        {
            var allTags = await _userTagRepo.GetProductTagsAsync(productId, ct);
            tags = allTags
                .Where(t => t.UserId == userId.Value && t.DeletedAt == null)
                .ToList();
        }
        else
        {
            tags = new List<UserTag>();
        }

        var tagList = tags
            .Select(t => new ProductTagItem
            {
                Id = t.Id.ToString(),
                Name = t.Title ?? ""
            })
            .ToList();

        rsp.SetData(tagList, "Get product tags successfully", 200);
        return rsp;
    }

    public async Task<BaseResponse> AddProductTagAsync(int userId, int productId, CreateProductTagReq request, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        // Check if product exists
        var product = await _productRepo.ReadAsync(productId, ct);
        if (product == null)
        {
            rsp.SetError("PRODUCT_NOT_FOUND", "Product not found", "Product not found", 404);
            return rsp;
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            rsp.SetError("INVALID_TAG", "Tag name is required", "Tag name cannot be empty", 400);
            return rsp;
        }

        var tagTitle = request.Name.Trim();

        // Check if tag already exists for this user and product
        var existingTag = await _userTagRepo.GetUserProductTagAsync(userId, productId, tagTitle, ct);
        if (existingTag != null)
        {
            rsp.SetError("TAG_EXISTS", "Tag already exists", "This tag has already been added", 409);
            return rsp;
        }

        var tag = new UserTag
        {
            UserId = userId,
            ProductId = productId,
            Title = tagTitle,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userTagRepo.CreateAsync(tag, ct);

        var tagRes = new ProductTagItem
        {
            Id = tag.Id.ToString(),
            Name = tag.Title ?? ""
        };

        rsp.SetData(tagRes, "Tag added successfully", 201);
        return rsp;
    }

    public async Task<BaseResponse> DeleteProductTagAsync(int userId, int tagId, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        var tag = await _userTagRepo.ReadAsync(tagId, ct);
        if (tag == null || tag.DeletedAt != null)
        {
            rsp.SetError("TAG_NOT_FOUND", "Tag not found", "Tag not found", 404);
            return rsp;
        }

        // Check ownership
        if (tag.UserId != userId)
        {
            rsp.SetError("FORBIDDEN", "You can only delete your own tags", "Forbidden", 403);
            return rsp;
        }

        tag.DeletedAt = DateTime.UtcNow;
        tag.UpdatedAt = DateTime.UtcNow;

        await _userTagRepo.UpdateAsync(tag, ct);

        rsp.SetData(null, "Tag deleted successfully", 200);
        return rsp;
    }
}

