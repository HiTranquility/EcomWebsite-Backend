using App.DAL.ProductModels;
using Bogus;

namespace App.DAL.DataSeedings.ProductExtensions;

public static class ProductImageExtension
{
    public static class ProductImage
    {
        public static List<ProductModels.ProductImage> GetSeedDataForProducts(
            IReadOnlyList<ProductModels.Product> products,
            int imagesPerProduct = 3)
        {
            var now = DateTime.UtcNow;
            var results = new List<ProductModels.ProductImage>(products.Count * imagesPerProduct);
            var faker = new Faker();

            foreach (var product in products)
            {
                var imageCount = faker.Random.Int(1, imagesPerProduct);
                for (var i = 0; i < imageCount; i++)
                {
                    results.Add(new ProductModels.ProductImage
                    {
                        ProductId = product.Id,
                        ImageUrl = faker.Image.PicsumUrl(),
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }
            }

            return results;
        }
    }
}

