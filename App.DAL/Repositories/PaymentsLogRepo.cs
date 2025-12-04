using App.DAL.OrderModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.Repositories;

public class PaymentsLogRepo : GenericRepo<EcomOrdersContext, PaymentsLog>
{
    public PaymentsLogRepo(EcomOrdersContext context) : base(context)
    {
    }
}


