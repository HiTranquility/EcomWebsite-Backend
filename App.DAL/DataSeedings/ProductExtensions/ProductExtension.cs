using App.DAL.ProductModels;
using Bogus;
using App.UTIL.Extensions;
using System.Text.Json;

namespace App.DAL.DataSeedings.ProductExtensions;

public static class ProductExtension
{
    public static class Product
    {
        public static List<ProductModels.Product> GetSeedData(
            int count,
            List<ProductModels.Manufacturer> manufacturers,
            List<ProductModels.ProductCategory> categories)
        {
            var now = DateTime.UtcNow;
            var faker = new Faker<ProductModels.Product>()
                .RuleFor(p => p.Title, f => f.Commerce.ProductName())
                .RuleFor(p => p.Slug, (f, p) => SlugServiceExtensions.ToSlug(p.Title ?? f.Commerce.ProductName()))
                .RuleFor(p => p.ShortDescription, f => f.Commerce.ProductDescription())
                .RuleFor(p => p.LongDescription, f => f.Lorem.Paragraphs(3))
                .RuleFor(p => p.OriginalPrice, f => f.Random.Decimal(10, 1000))
                .RuleFor(p => p.LastestPrice, (f, p) => p.OriginalPrice.HasValue 
                    ? f.Random.Decimal(p.OriginalPrice.Value * 0.5m, p.OriginalPrice.Value) 
                    : f.Random.Decimal(10, 1000))
                .RuleFor(p => p.MainImageUrl, f => f.Image.PicsumUrl())
                .RuleFor(p => p.ManufacturerId, f => manufacturers.Count == 0 
                    ? (int?)null 
                    : manufacturers[f.Random.Int(0, manufacturers.Count - 1)].Id)
                .RuleFor(p => p.ProductCategoryId, f => categories.Count == 0 
                    ? (int?)null 
                    : categories[f.Random.Int(0, categories.Count - 1)].Id)
                .RuleFor(p => p.Quantity, f => (uint?)f.Random.Int(0, 1000))
                .RuleFor(p => p.StockLeft, (f, p) => p.Quantity)
                .RuleFor(p => p.Size, f => f.Random.ArrayElement(new[] { "S", "M", "L", "XL", "XXL", null }))
                .RuleFor(p => p.IsFreeShipping, f => f.Random.Bool(0.3f))
                .RuleFor(p => p.IsFlashsale, f => f.Random.Bool(0.2f))
                .RuleFor(p => p.IsFeature, f => f.Random.Bool(0.15f))
                .RuleFor(p => p.IsSpecial, f => f.Random.Bool(0.1f))
                .RuleFor(p => p.IsWeekly, f => f.Random.Bool(0.1f))
                .RuleFor(p => p.IsToday, f => f.Random.Bool(0.05f))
                .RuleFor(p => p.IsDeal, f => f.Random.Bool(0.1f))
                .RuleFor(p => p.TotalStarRating, f => (decimal?)f.Random.Double(1, 5))
                .RuleFor(p => p.ReviewCount, f => f.Random.Int(0, 500))
                .RuleFor(p => p.Information, f => GenerateInformationJson(f))
                .RuleFor(p => p.CreatedAt, _ => now)
                .RuleFor(p => p.UpdatedAt, _ => now);
            return faker.Generate(count);
        }

        private static string? GenerateInformationJson(Faker f)
        {
            // Tạo một số thông tin mẫu ngẫu nhiên
            var informationItems = new List<Dictionary<string, string>>();
            
            var possibleInfo = new[]
            {
                new { Name = "Main Material", Values = new[] { "Cotton", "Polyester", "Wool", "Silk", "Linen", "Denim" } },
                new { Name = "Color", Values = new[] { "Black", "White", "Red", "Blue", "Green", "Yellow", "Gray", "Navy" } },
                new { Name = "Sleeves", Values = new[] { "Long Sleeve", "Short Sleeve", "Sleeveless", "3/4 Sleeve" } },
                new { Name = "Top Fit", Values = new[] { "Regular", "Slim Fit", "Loose Fit", "Oversized" } },
                new { Name = "Print", Values = new[] { "Not Printed", "Printed", "Pattern", "Solid" } },
                new { Name = "Neck", Values = new[] { "Round Neck", "V-Neck", "Collar", "Hood" } },
                new { Name = "Pieces Count", Values = new[] { "1 Piece", "2 Pieces", "3 Pieces" } },
                new { Name = "Occasion", Values = new[] { "Casual", "Formal", "Party", "Sports", "Office" } },
                new { Name = "Shipping Weight (kg)", Values = new[] { "0.3", "0.5", "0.8", "1.0", "1.5", "2.0" } },
                new { Name = "Care Instructions", Values = new[] { "Machine Wash", "Hand Wash", "Dry Clean Only", "Do Not Bleach" } }
            };

            // Chọn ngẫu nhiên 5-8 thông tin
            var selectedInfo = f.Random.Shuffle(possibleInfo).Take(f.Random.Int(5, 8));
            
            int idCounter = 1;
            foreach (var info in selectedInfo)
            {
                informationItems.Add(new Dictionary<string, string>
                {
                    { "id", idCounter++.ToString() },
                    { "name", info.Name },
                    { "value", f.Random.ArrayElement(info.Values) }
                });
            }

            try
            {
                return JsonSerializer.Serialize(informationItems);
            }
            catch
            {
                return null;
            }
        }
    }
}
