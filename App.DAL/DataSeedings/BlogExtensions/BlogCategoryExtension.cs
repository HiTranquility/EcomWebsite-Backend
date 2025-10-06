using App.DAL.BlogModels;
using Bogus;

namespace App.DAL.DataSeedings.BlogExtensions;

public static class BlogCategoryExtension
{
    public static class BlogCategory
    {
        public static List<BlogModels.BlogCategory> GetSeedData(int count)
        {
            var now = DateTime.UtcNow;
            var faker = new Faker<BlogModels.BlogCategory>()
                .RuleFor(c => c.Title, f => f.Commerce.Categories(1)[0])
                .RuleFor(c => c.CreatedAt, _ => now)
                .RuleFor(c => c.UpdatedAt, _ => now);
            return faker.Generate(count);
        }
    }
}


