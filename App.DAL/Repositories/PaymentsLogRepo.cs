using App.DAL.Interfaces;
using App.DAL.OrderModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.Repositories;

public class PaymentsLogRepo : GenericRepo<EcomOrdersContext, PaymentsLog>, IPaymentsLogRepo
{
    public PaymentsLogRepo(EcomOrdersContext context) : base(context)
    {
    }
}


