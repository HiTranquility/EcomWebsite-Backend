using App.BLL.Dtos.OrderDto;
using App.BLL.Dtos.OrderDto.Requests;
using App.BLL.Dtos.PaymentDto.Requests;
using App.BLL.Interfaces;
using App.UTIL.Abstractions.DTO.Response;
using App.UTIL.Extensions;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

/// <summary>
/// Order Controller - Handles order management operations
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/orders")]
public class OrderController : ControllerBase
{
    private readonly IOrderSvc _orderSvc;
    private readonly IPaymentSvc _paymentSvc;
    private readonly IOrderDeliverySvc _orderDeliverySvc;

    public OrderController(IOrderSvc orderSvc, IPaymentSvc paymentSvc, IOrderDeliverySvc orderDeliverySvc)
    {
        _orderSvc = orderSvc;
        _paymentSvc = paymentSvc;
        _orderDeliverySvc = orderDeliverySvc;
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderReq request, CancellationToken ct)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        BaseResponse rsp = await _orderSvc.CreateOrderAsync(userId.Value, request, ct);
        return StatusCode(rsp.Status, rsp);
    }

    /// <summary>
    /// Get order detail by ID
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOrderDetail(int id, CancellationToken ct)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        BaseResponse rsp = await _orderSvc.GetOrderDetailAsync(userId.Value, id, ct);
        return StatusCode(rsp.Status, rsp);
    }

    /// <summary>
    /// Get user's order list with filters
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOrderList([FromQuery] OrderFilter filter, CancellationToken ct)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        BaseResponse rsp = await _orderSvc.GetOrderListAsync(userId.Value, filter, ct);
        return StatusCode(rsp.Status, rsp);
    }

    /// <summary>
    /// Create payment for an order
    /// </summary>
    [HttpPost("{id:int}/payments")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreatePayment(int id, [FromBody] CreatePaymentReq request, CancellationToken ct)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        BaseResponse rsp = await _paymentSvc.CreatePaymentAsync(userId.Value, id, request, ct);
        return StatusCode(rsp.Status, rsp);
    }

    /// <summary>
    /// Get order delivery information
    /// </summary>
    [HttpGet("{id:int}/delivery")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOrderDelivery(int id, CancellationToken ct)
    {
        int? userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        BaseResponse rsp = await _orderDeliverySvc.GetOrderDeliveryAsync(userId.Value, id, ct);
        return StatusCode(rsp.Status, rsp);
    }
}