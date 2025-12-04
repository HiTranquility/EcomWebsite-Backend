using App.DAL.UserModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.Repositories;

public class WishlistRepo : GenericRepo<EcomUsersContext, Wishlist>
{
    public WishlistRepo(EcomUsersContext context) : base(context)
    {
    }
}


