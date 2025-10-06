using App.DAL.BlogModels;
using Bogus;

namespace App.DAL.DataSeedings.BlogExtensions;

public static class BlogCommentExtension
{
    public static class BlogComment
    {
        public static List<BlogModels.BlogComment> GetSeedDataForBlogs(List<BlogModels.Blog> blogs, int commentsPerBlog)
        {
            var now = DateTime.UtcNow;
            var faker = new Faker<BlogModels.BlogComment>()
                .RuleFor(c => c.FullName, f => f.Person.FullName)
                .RuleFor(c => c.Email, f => f.Internet.Email())
                .RuleFor(c => c.Content, f => f.Lorem.Sentences(2))
                .RuleFor(c => c.CreatedAt, _ => now)
                .RuleFor(c => c.UpdatedAt, _ => now);

            var comments = new List<BlogModels.BlogComment>();
            foreach (var blog in blogs)
            {
                comments.AddRange(Enumerable.Range(0, commentsPerBlog).Select(_ =>
                {
                    var c = faker.Generate();
                    c.BlogId = blog.Id;
                    return c;
                }));
            }
            return comments;
        }
    }
}


