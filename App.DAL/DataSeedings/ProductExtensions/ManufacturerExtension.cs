using App.DAL.ProductModels;
using Bogus;

namespace App.DAL.DataSeedings.ProductExtensions;

public static class ManufacturerExtension
{
    public static class Manufacturer
    {
        public static List<ProductModels.Manufacturer> GetSeedData(int count)
        {
            var now = DateTime.UtcNow;
            var faker = new Faker<ProductModels.Manufacturer>()
                .RuleFor(m => m.Name, f => f.Company.CompanyName())
                .RuleFor(m => m.Total, f => f.Random.Int(0, 1000))
                .RuleFor(m => m.CreatedAt, _ => now)
                .RuleFor(m => m.UpdatedAt, _ => now);
            return faker.Generate(count);
        }
    }
}

