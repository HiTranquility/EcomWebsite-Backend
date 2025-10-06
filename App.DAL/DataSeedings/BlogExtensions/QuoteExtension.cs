using App.DAL.BlogModels;
using Bogus;

namespace App.DAL.DataSeedings.BlogExtensions;

public static class QuoteExtension
{
    public static class Quote
    {
        public static List<BlogModels.Quote> GetSeedData(int count)
        {
            var now = DateTime.UtcNow;
            var faker = new Faker<BlogModels.Quote>()
                .RuleFor(q => q.Author, f => f.Person.FullName)
                .RuleFor(q => q.Content, f => f.Lorem.Sentence(10))
                .RuleFor(q => q.CreatedAt, _ => now)
                .RuleFor(q => q.UpdatedAt, _ => now);
            return faker.Generate(count);
        }
    }
}


