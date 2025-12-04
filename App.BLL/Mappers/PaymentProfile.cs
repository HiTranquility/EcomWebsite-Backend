using App.BLL.Dtos.PaymentDto.Requests;
using App.BLL.Dtos.PaymentDto.Results;
using App.DAL.OrderModels;
using AutoMapper;

namespace App.BLL.Mappers;

public class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        // INPUT mapping: CreatePaymentReq → Transaction
        CreateMap<CreatePaymentReq, Transaction>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.OrderId, o => o.Ignore())
            .ForMember(d => d.Amount, o => o.Ignore())
            .ForMember(d => d.Method, o => o.MapFrom(s => 
                string.IsNullOrWhiteSpace(s.Method) ? "COD" : s.Method.Trim().ToUpperInvariant()))
            .ForMember(d => d.Status, o => o.MapFrom(_ => "pending"))
            .ForMember(d => d.GatewayTransactionCode, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.Order, o => o.Ignore())
            .ForMember(d => d.PaymentsLogs, o => o.Ignore());

        // OUTPUT mapping: Transaction → PaymentRes
        CreateMap<Transaction, PaymentRes>()
            .ForMember(d => d.TransactionId, o => o.MapFrom(s => s.Id));
    }
}

