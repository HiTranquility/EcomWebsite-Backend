using App.DAL.ProductModels;
using Bogus;

namespace App.DAL.DataSeedings.ProductExtensions;

public static class ProductCategoryExtension
{
    public static class ProductCategory
    {
        public static List<ProductModels.ProductCategory> GetSeedData(int count)
        {
            var now = DateTime.UtcNow;
            var faker = new Faker<ProductModels.ProductCategory>()
                .RuleFor(c => c.Title, f => f.Commerce.Categories(1)[0])
                .RuleFor(c => c.Parent, _ => (int?)null) // Will be set after saving to DB
                .RuleFor(c => c.CreatedAt, _ => now)
                .RuleFor(c => c.UpdatedAt, _ => now);
            return faker.Generate(count);
        }

        /// <summary>
        /// Assigns parent relationships to categories after they've been saved to DB (have IDs)
        /// </summary>
        public static void AssignParentRelationships(List<ProductModels.ProductCategory> categories)
        {
            if (categories.Count <= 1) return;

            var faker = new Faker();
            // About 30% of categories will have a parent (creating a hierarchy)
            var categoriesWithParent = categories.Skip(1).OrderBy(_ => faker.Random.Int()).Take((int)(categories.Count * 0.3)).ToList();
            
            foreach (var category in categoriesWithParent)
            {
                // Pick a random parent from categories that don't have a parent yet
                var possibleParents = categories.Where(c => c != category && c.Parent == null && c.Id != category.Id).ToList();
                if (possibleParents.Count > 0)
                {
                    category.Parent = faker.PickRandom(possibleParents).Id;
                }
            }
        }
    }
}

