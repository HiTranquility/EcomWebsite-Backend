using App.DAL.BlogModels;
using App.UTIL.Abstractions.DAL;

namespace App.DAL.DataSeedings;

public sealed class BlogSchema : SeedSchema<EcomBlogsContext>
{
    public int BlogsCount { get; set; }
    public int CategoriesCount { get; set; }
    public int TagsCount { get; set; }
    public int CommentsPerBlog { get; set; }
    public int CategoriesPerBlog { get; set; }
    public int TagsPerBlog { get; set; }
    public int QuotesCount { get; set; } 
    public int MaxExtraVariantsPerBlog { get; set; }
    internal EcomBlogsContext Db => Context;

    public BlogSchema(EcomBlogsContext context) : base(context) { }

    public override Task RunAsync(CancellationToken ct) =>
        ModelBuilderExtensions.SeedBlogTablesAsync(this, ct);
}