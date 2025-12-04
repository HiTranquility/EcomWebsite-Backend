using App.BLL.Dtos.OrderDto;
using App.BLL.Dtos.OrderDto.Requests;
using App.BLL.Dtos.OrderDto.Results;
using App.BLL.Dtos.OrderDto.Shares;
using App.DAL.OrderModels;
using App.DAL.Repositories;
using App.UTIL.Abstractions.BLL;
using App.UTIL.Abstractions.DTO.Response;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace App.BLL.Services;

public class OrderSvc : GenericSvc<OrderRepo, Order>
{
    private readonly CartSvc _cartSvc;

    public OrderSvc(OrderRepo repo, CartSvc cartSvc, IMapper mapper) : base(repo, mapper)
    {
        _cartSvc = cartSvc;
    }

    public async Task<BaseResponse> CreateOrderAsync(int userId, CreateOrderReq request, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        if (request.CartId <= 0)
        {
            rsp.SetError("INVALID_CART", "CartId is required", "Invalid cart", 400);
            return rsp;
        }

        var cart = await _cartSvc.GetActiveCartWithItemsAsync(userId, request.CartId, ct);
        if (cart == null || cart.CartItems.Count == 0)
        {
            rsp.SetError("CART_NOT_FOUND", "Cart not found or empty", "Cart not found or empty", 404);
            return rsp;
        }

        var totalAmount = cart.CartItems.Sum(i => i.Subtotal);
        
        // Calculate shipping fee based on delivery type if not provided
        decimal shippingFee = request.ShippingFee ?? 0m;
        if (shippingFee == 0 && !string.IsNullOrWhiteSpace(request.DeliveryType))
        {
            shippingFee = request.DeliveryType.Trim().ToLowerInvariant() switch
            {
                "fast" => 10m, // Fast delivery costs $10
                "standard" => 0m, // Standard delivery is free
                _ => 0m
            };
        }
        
        var discountAmount = request.DiscountAmount ?? 0m;
        var finalPrice = totalAmount + shippingFee - discountAmount;

        var order = _mapper.Map<Order>(request);
        order.UserId = userId;
        order.CartId = cart.Id;
        order.TotalAmount = totalAmount;
        order.ShippingFee = shippingFee;
        order.DiscountAmount = discountAmount;
        order.FinalPrice = finalPrice;
        order.CreatedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        var items = cart.CartItems.Select(ci => new OrderItem
        {
            ProductId = ci.ProductId,
            VariantId = ci.VariantId,
            Quantity = ci.Quantity ?? 1,
            PriceAtTime = ci.PriceAtTime,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        order = await _repo.CreateOrderWithItemsAsync(order, items, ct);
        await _cartSvc.MarkCheckedOutAsync(cart.Id, ct);

        var detail = await _repo.GetOrderWithDetailsAsync(order.Id, userId, ct);
        if (detail == null)
        {
            rsp.SetError("ORDER_NOT_FOUND", "Order not found after creation", "Order not found after creation", 404);
            return rsp;
        }

        var mapped = _mapper.Map<OrderDetailRes>(detail);
        rsp.SetData(mapped, "Order created successfully", 201);
        return rsp;
    }

    public async Task<BaseResponse> GetOrderDetailAsync(int userId, int orderId, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        var order = await _repo.GetOrderWithDetailsAsync(orderId, userId, ct);
        if (order == null)
        {
            rsp.SetError("ORDER_NOT_FOUND", "Order not found", "Order not found", 404);
            return rsp;
        }

        var mapped = _mapper.Map<OrderDetailRes>(order);
        rsp.SetData(mapped, "Get order detail successfully", 200);
        return rsp;
    }

    public async Task<BaseResponse> GetOrderListAsync(int userId, OrderFilter filter, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();
        filter.Normalize();

        var query = _repo.All
            .AsNoTracking()
            .TagWith("OrderSvc.GetOrderListAsync")
            .Where(o => o.UserId == userId);

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var status = filter.Status.Trim();
            query = query.Where(o => o.Status == status);
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ProjectTo<OrderListItemRes>(_mapper.ConfigurationProvider)
            .ToListAsync(ct);

        rsp.SetData(new
        {
            Total = total,
            Items = items,
            Page = filter.Page,
            PageSize = filter.PageSize
        }, "Get orders successfully", 200);

        return rsp;
    }
}


