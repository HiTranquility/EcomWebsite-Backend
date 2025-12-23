using App.DAL.Interfaces;
using App.DAL.BlogModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.Repositories;

public class BlogCommentRepo : GenericRepo<EcomBlogsContext, BlogComment>, IBlogCommentRepo
{
    public BlogCommentRepo(EcomBlogsContext context) : base(context) {}
}