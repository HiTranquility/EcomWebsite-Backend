using App.BLL.Dtos.OrderDto;
using App.BLL.Dtos.OrderDto.Requests;
using App.BLL.Dtos.PaymentDto.Requests;
using App.BLL.Services;
using App.UTIL.Abstractions.DTO.Response;
using App.UTIL.Extensions;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/orders")]
public class OrderController : ControllerBase
{
    private readonly OrderSvc _orderSvc;
    private readonly PaymentSvc _paymentSvc;
    private readonly OrderDeliverySvc _orderDeliverySvc;

    public OrderController(OrderSvc orderSvc, PaymentSvc paymentSvc, OrderDeliverySvc orderDeliverySvc)
    {
        _orderSvc = orderSvc;
        _paymentSvc = paymentSvc;
        _orderDeliverySvc = orderDeliverySvc;
    }

    [HttpPost]
    [Authorize]
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

    [HttpGet("{id:int}")]
    [Authorize]
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

    [HttpGet]
    [Authorize]
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

    [HttpPost("{id:int}/payments")]
    [Authorize]
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

    [HttpGet("{id:int}/delivery")]
    [Authorize]
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