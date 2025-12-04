using Bogus;
using App.UTIL.Extensions;

namespace App.DAL.DataSeedings.BlogExtensions;

public static class BlogExtension
{
    public static class Blog
    {
        public static List<BlogModels.Blog> GetSeedData(int count, List<BlogModels.Quote> quotes)
        {
            var now = DateTime.UtcNow;
            var faker = new Faker<BlogModels.Blog>()
                .RuleFor(b => b.Title, f => f.Lorem.Sentence(6))
                .RuleFor(b => b.Slug, (f, b) => SlugServiceExtensions.ToSlug(b.Title ?? f.Lorem.Sentence(6)))
                .RuleFor(b => b.Content, f => f.Lorem.Paragraphs(2))
                .RuleFor(b => b.Author, f => f.Person.FullName)
                .RuleFor(b => b.QuoteId, f => quotes.Count == 0 ? (int?)null : quotes[f.Random.Int(0, quotes.Count - 1)].Id)
                .RuleFor(b => b.CommentCount, _ => 0)
                .RuleFor(b => b.CreatedAt, _ => now)
                .RuleFor(b => b.UpdatedAt, _ => now);
            return faker.Generate(count);
        }
    }
}


