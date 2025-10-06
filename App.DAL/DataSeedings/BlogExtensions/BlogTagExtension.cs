using App.DAL.BlogModels;
using Bogus;

namespace App.DAL.DataSeedings.BlogExtensions;

public static class BlogTagExtension
{
    public static class BlogTag
    {
        public static List<BlogModels.BlogTag> GetSeedData(int count)
        {
            var now = DateTime.UtcNow;
            var faker = new Faker<BlogModels.BlogTag>()
                .RuleFor(t => t.Title, f => f.Lorem.Word())
                .RuleFor(t => t.CreatedAt, _ => now)
                .RuleFor(t => t.UpdatedAt, _ => now);
            return faker.Generate(count);
        }
    }
}


