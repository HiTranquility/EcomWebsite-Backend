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
                .RuleFor(c => c.Content, f => f.Lorem.Sentences(2))
                .RuleFor(c => c.CreatedAt, _ => now)
                .RuleFor(c => c.UpdatedAt, _ => now);

            var comments = new List<BlogModels.BlogComment>();
            var rnd = new Random();
            foreach (var blog in blogs)
            {
                // Tạo các comment cấp 1 (parent == null)
                var parents = Enumerable.Range(0, commentsPerBlog).Select(_ =>
                {
                    var c = faker.Generate();
                    c.BlogId = blog.Id;
                    return c;
                }).ToList();

                comments.AddRange(parents);

                // Tạo 0..2 replies cho mỗi parent
                foreach (var parent in parents)
                {
                    var childCount = rnd.Next(0, 3);
                    for (var i = 0; i < childCount; i++)
                    {
                        var child = faker.Generate();
                        child.BlogId = blog.Id;
                        child.ParentNavigation = parent; // EF sẽ set FK Parent khi save
                        comments.Add(child);
                    }
                }
            }
            return comments;
        }
    }
}


