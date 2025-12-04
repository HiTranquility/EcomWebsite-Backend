using App.DAL.ProductModels;
using Bogus;

namespace App.DAL.DataSeedings.ProductExtensions;

public static class ProductReviewExtension
{
    public static class ProductReview
    {
        public static List<ProductModels.ProductReview> GetSeedDataForProducts(
            IReadOnlyList<ProductModels.Product> products,
            int reviewsPerProduct = 5)
        {
            var now = DateTime.UtcNow;
            var results = new List<ProductModels.ProductReview>(products.Count * reviewsPerProduct);
            var faker = new Faker();

            foreach (var product in products)
            {
                var reviewCount = faker.Random.Int(0, reviewsPerProduct);
                for (var i = 0; i < reviewCount; i++)
                {
                    results.Add(new ProductModels.ProductReview
                    {
                        ProductId = product.Id,
                        StarRating = (uint?)faker.Random.Int(1, 5),
                        Content = faker.Lorem.Paragraph(),
                        Email = faker.Internet.Email(),
                        FullName = faker.Person.FullName,
                        UserId = null,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }
            }

            return results;
        }
    }
}

