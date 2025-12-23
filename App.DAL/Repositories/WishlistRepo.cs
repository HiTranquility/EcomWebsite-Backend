using App.DAL.Interfaces;
using App.DAL.UserModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.Repositories;

public class WishlistRepo : GenericRepo<EcomUsersContext, Wishlist>, IWishlistRepo
{
    public WishlistRepo(EcomUsersContext context) : base(context)
    {
    }
}


