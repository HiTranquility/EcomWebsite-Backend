using App.DAL.BlogModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.Repositories;

public class BlogRepo : GenericRepo<EcomBlogsContext, Blog>
{
    public BlogRepo (EcomBlogsContext context) : base(context)
    {
    }
}