using App.DAL.BlogModels;
using Bogus;

namespace App.DAL.DataSeedings.BlogExtensions;

public static class BlogCategoryExtension
{
    public static class BlogCategory
    {
        public static List<BlogModels.BlogCategory> GetSeedData(int count, IReadOnlyList<BlogModels.Blog> blogs)
        {
            var now = DateTime.UtcNow;
            var faker = new Faker();
            var results = new List<BlogModels.BlogCategory>();
            
            for (var i = 0; i < count; i++)
            {
                int? blogId = null;
                if (blogs.Count > 0)
                {
                    blogId = blogs[faker.Random.Int(0, blogs.Count - 1)].Id;
                }
                
                results.Add(new BlogModels.BlogCategory
                {
                    Title = faker.Commerce.Categories(1)[0],
                    BlogId = blogId,
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
            
            return results;
        }
    }
}


