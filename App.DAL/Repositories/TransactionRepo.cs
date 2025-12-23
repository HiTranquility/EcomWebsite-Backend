using App.DAL.Interfaces;
using App.DAL.OrderModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.Repositories;

public class TransactionRepo : GenericRepo<EcomOrdersContext, Transaction>, ITransactionRepo
{
    public TransactionRepo(EcomOrdersContext context) : base(context)
    {
    }
}


