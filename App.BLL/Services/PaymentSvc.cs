using App.BLL.Dtos.PaymentDto.Requests;
using App.BLL.Dtos.PaymentDto.Results;
using App.DAL.OrderModels;
using App.DAL.Repositories;
using App.UTIL.Abstractions.BLL;
using App.UTIL.Abstractions.DTO.Response;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace App.BLL.Services;

public class PaymentSvc : GenericSvc<TransactionRepo, Transaction>
{
    private readonly OrderRepo _orderRepo;
    private readonly PaymentsLogRepo _paymentsLogRepo;

    public PaymentSvc(
        TransactionRepo repo,
        OrderRepo orderRepo,
        PaymentsLogRepo paymentsLogRepo,
        IMapper mapper) : base(repo, mapper)
    {
        _orderRepo = orderRepo;
        _paymentsLogRepo = paymentsLogRepo;
    }

    public async Task<BaseResponse> CreatePaymentAsync(int userId, int orderId, CreatePaymentReq request, CancellationToken ct = default)
    {
        var rsp = new BaseResponse();

        var order = await _orderRepo.All
            .AsNoTracking()
            .TagWith("PaymentSvc.CreatePaymentAsync.GetOrder")
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, ct);

        if (order == null)
        {
            rsp.SetError("ORDER_NOT_FOUND", "Order not found", "Order not found", 404);
            return rsp;
        }

        if (string.Equals(order.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
        {
            rsp.SetError("ORDER_ALREADY_PAID", "Order is already paid", "Order already paid", 400);
            return rsp;
        }

        var tx = _mapper.Map<Transaction>(request);
        tx.OrderId = order.Id;
        tx.Amount = order.FinalPrice;
        tx.CreatedAt = DateTime.UtcNow;
        tx.UpdatedAt = DateTime.UtcNow;

        await _repo.CreateAsync(tx, ct);

        var mapped = _mapper.Map<PaymentRes>(tx);
        rsp.SetData(mapped, "Payment initialized successfully", 201);

        return rsp;
    }
}


