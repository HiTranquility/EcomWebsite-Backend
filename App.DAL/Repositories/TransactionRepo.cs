using App.DAL.OrderModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.Repositories;

public class TransactionRepo : GenericRepo<EcomOrdersContext, Transaction>
{
    public TransactionRepo(EcomOrdersContext context) : base(context)
    {
    }
}


