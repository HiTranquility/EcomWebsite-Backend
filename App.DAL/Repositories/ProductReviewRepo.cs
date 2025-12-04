using App.DAL.ProductModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.Repositories;

public class ProductReviewRepo : GenericRepo<EcomProductsContext, ProductReview>
{
    public ProductReviewRepo(EcomProductsContext context) : base(context)
    {
    }
}

