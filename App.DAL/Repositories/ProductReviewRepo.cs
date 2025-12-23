using App.DAL.Interfaces;
using App.DAL.ProductModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.Repositories;

public class ProductReviewRepo : GenericRepo<EcomProductsContext, ProductReview>, IProductReviewRepo
{
    public ProductReviewRepo(EcomProductsContext context) : base(context)
    {
    }
}

