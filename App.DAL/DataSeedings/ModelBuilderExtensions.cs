using System.Diagnostics;
using App.DAL.UserModels;
using Microsoft.EntityFrameworkCore;
using App.DAL.BlogModels;
using App.DAL.DataSeedings.BlogExtensions;
using App.DAL.DataSeedings.UserExtensions;
using App.UTIL.Abstractions.DAL;
using App.DAL.ProductModels;
using App.DAL.DataSeedings.ProductExtensions;
using App.UTIL.Extensions;
using App.DAL.OrderModels;

namespace App.DAL.DataSeedings;

public static class ModelBuilderExtensions
{
    public static async Task SeedUserTablesAsync(UserSchema schema, CancellationToken ct = default)
    {
        await schema.WithSeedContextAsync(async () =>
        {
            var db = schema.Db;
            var inc = schema.Include;

            static async Task Section(
                HashSet<string>? include, string key,
                Func<Task<bool>> hasAnyAsync,
                Func<Task> seedAsync,
                bool diagnostics = false)
            {
                if (!SeedSchema<EcomUsersContext>.ShouldInclude(include, key)) return;
                if (await hasAnyAsync()) return;
                if (!diagnostics)
                {
                    await seedAsync();
                    return;
                }
                var sw = Stopwatch.StartNew();
                await seedAsync();
                sw.Stop();
                Console.WriteLine($"[Seed][Users] {key} took {sw.ElapsedMilliseconds} ms");
            }

            static async Task Commit(DbContext db, CancellationToken ct)
            {
                db.ChangeTracker.DetectChanges();
                await db.SaveChangesAsync(false, ct);
                db.ChangeTracker.Clear();
            }

            await Section(inc, "Permissions",
                () => db.Permissions.AsNoTracking().AnyAsync(ct),
                async () =>
                {
                    var permissions = PermissionExtension.Permission.GetSeedData();
                    await db.Permissions.AddRangeAsync(permissions, ct);
                    await Commit(db, ct);
                }, schema.EnableDiagnostics);

            await Section(inc, "Roles",
                () => db.Roles.AsNoTracking().AnyAsync(ct),
                async () =>
                {
                    var permissionsByKey = await db.Permissions
                        .AsTracking()
                        .ToDictionaryAsync(p => p.Key, StringComparer.OrdinalIgnoreCase, ct);
                    if (permissionsByKey.Count == 0) return;

                    var roles = RoleExtension.Role.GetSeedData(permissionsByKey);
                    if (roles.Count == 0) return;

                    await db.Roles.AddRangeAsync(roles, ct);
                    await Commit(db, ct);
                }, schema.EnableDiagnostics);

            await Section(inc, "Users",
                () => db.Users.AsNoTracking().AnyAsync(ct),
                async () =>
                {
                    for (var created = 0; created < schema.UsersCount; created += schema.BatchSize)
                    {
                        var take = Math.Min(schema.BatchSize, schema.UsersCount - created);
                        var batch = UserExtension.User.GetSeedData(take);
                        await db.Users.AddRangeAsync(batch, ct);
                        await Commit(db, ct);
                    }
                }, schema.EnableDiagnostics);

            var usersAll = await db.Users.AsNoTracking().ToListAsync(ct);
            var rolesAll = await db.Roles.AsNoTracking().ToListAsync(ct);

            await Section(inc, "UserRoles",
                () => db.Set<Dictionary<string, object>>("UserRole")
                    .AsNoTracking()
                    .AnyAsync(ct),
                async () =>
                {
                    var seeds = UserRoleExtension.GetSeedDataForUsers(usersAll, rolesAll);
                    if (seeds.Count == 0) return;

                    foreach (var chunk in SeedSchema<EcomUsersContext>.Chunk(seeds.ToList(), schema.BatchSize))
                    {
                        var chunkList = chunk.ToList();
                        if (chunkList.Count == 0) continue;

                        var parameters = new List<object>(chunkList.Count * 2);
                        var values = new List<string>(chunkList.Count);

                        for (var i = 0; i < chunkList.Count; i++)
                        {
                            var seed = chunkList[i];
                            values.Add($"(@p{parameters.Count}, @p{parameters.Count + 1})");
                            parameters.Add(seed.UserId);
                            parameters.Add(seed.RoleId);
                        }

                        var sql = $"INSERT INTO user_roles (user_id, role_id) VALUES {string.Join(", ", values)};";
                        await db.Database.ExecuteSqlRawAsync(sql, parameters.ToArray());
                    }
                }, schema.EnableDiagnostics);

            await Section(inc, "AddressBooks",
                () => db.AddressBooks.AsNoTracking().AnyAsync(ct),
                async () =>
                {
                    foreach (var chunk in SeedSchema<EcomUsersContext>.Chunk(usersAll, schema.BatchSize))
                    {
                        var items = AddressBookExtension.AddressBook.GetSeedDataForUsers(chunk, schema.AddressBooksPerUser);
                        if (items.Count == 0) continue;
                        await db.AddressBooks.AddRangeAsync(items, ct);
                        await Commit(db, ct);
                    }
                }, schema.EnableDiagnostics);

            await Section(inc, "RefreshTokens",
                () => db.RefreshTokens.AsNoTracking().AnyAsync(ct),
                async () =>
                {
                    foreach (var chunk in SeedSchema<EcomUsersContext>.Chunk(usersAll, schema.BatchSize))
                    {
                        var items = RefreshTokenExtension.RefreshToken.GetSeedDataForUsers(chunk, schema.RefreshTokensPerUser);
                        if (items.Count == 0) continue;
                        await db.RefreshTokens.AddRangeAsync(items, ct);
                        await Commit(db, ct);
                    }
                }, schema.EnableDiagnostics);

            await Section(inc, "Carts",
                () => db.Carts.AsNoTracking().AnyAsync(ct),
                async () =>
                {
                    foreach (var chunk in SeedSchema<EcomUsersContext>.Chunk(usersAll, schema.BatchSize))
                    {
                        var items = CartExtension.Cart.GetSeedDataForUsers(chunk, schema.CartsPerUser);
                        if (items.Count == 0) continue;
                        await db.Carts.AddRangeAsync(items, ct);
                        await Commit(db, ct);
                    }
                }, schema.EnableDiagnostics);

            await Section(inc, "Contacts",
                () => db.Contacts.AsNoTracking().AnyAsync(ct),
                async () =>
                {
                    for (var created = 0; created < schema.ContactsCount; created += schema.BatchSize)
                    {
                        var take = Math.Min(schema.BatchSize, schema.ContactsCount - created);
                        var items = ContactExtension.Contact.GetSeedData(take);
                        if (items.Count == 0) continue;
                        await db.Contacts.AddRangeAsync(items, ct);
                        await Commit(db, ct);
                    }
                }, schema.EnableDiagnostics);

            await Section(inc, "Newsletters",
                () => db.Newsletters.AsNoTracking().AnyAsync(ct),
                async () =>
                {
                    foreach (var chunk in SeedSchema<EcomUsersContext>.Chunk(usersAll, schema.BatchSize))
                    {
                        var items = NewsletterExtension.Newsletter.GetSeedDataForUsers(chunk, schema.NewslettersProbabilityPerUser);
                        if (items.Count == 0) continue;
                        await db.Newsletters.AddRangeAsync(items, ct);
                        await Commit(db, ct);
                    }
                }, schema.EnableDiagnostics);

            await Section(inc, "UserTags",
                () => db.UserTags.AsNoTracking().AnyAsync(ct),
                async () =>
                {
                    foreach (var chunk in SeedSchema<EcomUsersContext>.Chunk(usersAll, schema.BatchSize))
                    {
                        var items = UserTagExtension.UserTag.GetSeedDataForUsers(chunk, schema.UserTagsPerUser);
                        if (items.Count == 0) continue;
                        await db.UserTags.AddRangeAsync(items, ct);
                        await Commit(db, ct);
                    }
                }, schema.EnableDiagnostics);

            await Section(inc, "Wishlists",
                () => db.Wishlists.AsNoTracking().AnyAsync(ct),
                async () =>
                {
                    foreach (var chunk in SeedSchema<EcomUsersContext>.Chunk(usersAll, schema.BatchSize))
                    {
                        var items = WishlistExtension.Wishlist.GetSeedDataForUsers(chunk, schema.WishlistsPerUser);
                        if (items.Count == 0) continue;
                        await db.Wishlists.AddRangeAsync(items, ct);
                        await Commit(db, ct);
                    }
                }, schema.EnableDiagnostics);
        });
    }
    public static async Task SeedProductTablesAsync(ProductSchema schema, CancellationToken ct = default)
    {
        await schema.WithSeedContextAsync(async () =>
        {
            var db = schema.Db;
            var inc = schema.Include;
            var now = DateTime.UtcNow;

            static async Task Section(
                HashSet<string>? include, string key,
                Func<Task<bool>> hasAnyAsync,
                Func<Task> seedAsync,
                bool diagnostics = false)
            {
                var allow = SeedSchema<EcomProductsContext>.ShouldInclude(include, key) ||
                            SeedSchema<EcomProductsContext>.ShouldInclude(include, "Products");
                if (!allow) return;
                if (await hasAnyAsync()) return;
                if (!diagnostics)
                {
                    await seedAsync();
                    return;
                }
                var sw = Stopwatch.StartNew();
                await seedAsync();
                sw.Stop();
                Console.WriteLine($"[Seed][Products] {key} took {sw.ElapsedMilliseconds} ms");
            }

            static async Task Commit(EcomProductsContext db, CancellationToken ct)
            {
                db.ChangeTracker.DetectChanges();
                await db.SaveChangesAsync(false, ct);
                db.ChangeTracker.Clear();
            }

            await Section(inc, "ProductManufacturers",
                () => db.Manufacturers.AsNoTracking().AnyAsync(ct),
                async () =>
                {
                    for (var created = 0; created < schema.ManufacturersCount; created += schema.BatchSize)
                    {
                        var take = Math.Min(schema.BatchSize, schema.ManufacturersCount - created);
                        var batch = ManufacturerExtension.Manufacturer.GetSeedData(take);
                        await db.Manufacturers.AddRangeAsync(batch, ct);
                        await Commit(db, ct);
                    }
                }, schema.EnableDiagnostics);
            var allManufacturers = await db.Manufacturers.AsNoTracking().ToListAsync(ct);

            await Section(inc, "ProductCategories",
                () => db.ProductCategories.AsNoTracking().AnyAsync(ct),
                async () =>
                {
                    // First, seed parent categories (about 70% of total)
                    var parentCount = (int)(schema.CategoriesCount * 0.7);
                    var childCount = schema.CategoriesCount - parentCount;
                    
                    // Seed parent categories (no parent)
                    for (var created = 0; created < parentCount; created += schema.BatchSize)
                    {
                        var take = Math.Min(schema.BatchSize, parentCount - created);
                        var batch = ProductCategoryExtension.ProductCategory.GetSeedData(take);
                        await db.ProductCategories.AddRangeAsync(batch, ct);
                        await Commit(db, ct);
                    }

                    // Get all parent categories to assign as parents
                    var parentCategories = await db.ProductCategories.AsNoTracking().Where(c => c.Parent == null).ToListAsync(ct);
                    
                    // Seed child categories (with parent)
                    if (parentCategories.Count > 0 && childCount > 0)
                    {
                        var faker = new Bogus.Faker();
                        var now = DateTime.UtcNow;
                        
                        for (var created = 0; created < childCount; created += schema.BatchSize)
                        {
                            var take = Math.Min(schema.BatchSize, childCount - created);
                            var childBatch = new List<ProductModels.ProductCategory>();
                            
                            for (var i = 0; i < take; i++)
                            {
                                var parent = faker.PickRandom(parentCategories);
                                childBatch.Add(new ProductModels.ProductCategory
                                {
                                    Title = faker.Commerce.Categories(1)[0],
                                    Parent = parent.Id,
                                    CreatedAt = now,
                                    UpdatedAt = now
                                });
                            }
                            
                            await db.ProductCategories.AddRangeAsync(childBatch, ct);
                            await Commit(db, ct);
                        }
                    }
                }, schema.EnableDiagnostics);
            var allCategories = await db.ProductCategories.AsNoTracking().ToListAsync(ct);

            await Section(inc, "Products",
                () => db.Products.AsNoTracking().AnyAsync(ct),
                async () =>
                {
                    if (allManufacturers.Count == 0 || allCategories.Count == 0) return;

                    for (var created = 0; created < schema.ProductsCount; created += schema.BatchSize)
                    {
                        var take = Math.Min(schema.BatchSize, schema.ProductsCount - created);
                        var batch = ProductExtension.Product.GetSeedData(take, allManufacturers, allCategories);
                        await db.Products.AddRangeAsync(batch, ct);
                        await Commit(db, ct);
                    }
                }, schema.EnableDiagnostics);
            var allProducts = await db.Products.AsNoTracking().ToListAsync(ct);

            await Section(inc, "ProductImages",
                () => db.ProductImages.AsNoTracking().AnyAsync(ct),
                async () =>
                {
                    if (allProducts.Count == 0) return;
                    foreach (var chunk in SeedSchema<EcomProductsContext>.Chunk(allProducts, schema.BatchSize))
                    {
                        var items = ProductImageExtension.ProductImage.GetSeedDataForProducts(chunk, schema.ImagesPerProduct);
                        if (items.Count == 0) continue;
                        await db.ProductImages.AddRangeAsync(items, ct);
                        await Commit(db, ct);
                    }
                }, schema.EnableDiagnostics);

            await Section(inc, "ProductReviews",
                () => db.ProductReviews.AsNoTracking().AnyAsync(ct),
                async () =>
                {
                    if (allProducts.Count == 0) return;
                    foreach (var chunk in SeedSchema<EcomProductsContext>.Chunk(allProducts, schema.BatchSize))
                    {
                        var items = ProductReviewExtension.ProductReview.GetSeedDataForProducts(chunk, 5);
                        if (items.Count == 0) continue;
                        await db.ProductReviews.AddRangeAsync(items, ct);
                        await Commit(db, ct);
                    }
                }, schema.EnableDiagnostics);
        });
    }

    public static async Task SeedOrderTablesAsync(OrderSchema schema, CancellationToken ct = default)
    {
        await schema.WithSeedContextAsync(async () =>
        {
            var db = schema.Db;
            var inc = schema.Include;

            static async Task Section(
                HashSet<string>? include, string key,
                Func<Task<bool>> hasAnyAsync,
                Func<Task> seedAsync,
                bool diagnostics = false)
            {
                var allow = SeedSchema<EcomOrdersContext>.ShouldInclude(include, key) ||
                            SeedSchema<EcomOrdersContext>.ShouldInclude(include, "Orders");
                if (!allow) return;
                if (await hasAnyAsync()) return;
                if (!diagnostics)
                {
                    await seedAsync();
                    return;
                }
                var sw = Stopwatch.StartNew();
                await seedAsync();
                sw.Stop();
                Console.WriteLine($"[Seed][Orders] {key} took {sw.ElapsedMilliseconds} ms");
            }

            static async Task Commit(EcomOrdersContext db, CancellationToken ct)
            {
                db.ChangeTracker.DetectChanges();
                await db.SaveChangesAsync(false, ct);
                db.ChangeTracker.Clear();
            }

            await Section(inc, "Carts",
                () => db.Carts.AsNoTracking().AnyAsync(ct),
                async () =>
                {
                    var rnd = new Random();
                    var carts = new List<App.DAL.OrderModels.Cart>();
                    for (var i = 1; i <= schema.OrdersCount; i++)
                    {
                        carts.Add(new App.DAL.OrderModels.Cart
                        {
                            UserId = rnd.Next(1, 201),
                            TotalPrice = 0,
                            TotalQuantity = 0,
                            Status = "checked_out",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }
                    await db.Carts.AddRangeAsync(carts, ct);
                    await Commit(db, ct);
                }, schema.EnableDiagnostics);

            await Section(inc, "Orders",
                () => db.Orders.AsNoTracking().AnyAsync(ct),
                async () =>
                {
                    var rnd = new Random();
                    var carts = await db.Carts.AsNoTracking().ToListAsync(ct);
                    var orders = new List<Order>();
                    var orderItems = new List<OrderItem>();

                    foreach (var cart in carts)
                    {
                        var itemCount = rnd.Next(1, Math.Max(2, schema.MaxItemsPerOrder));
                        decimal total = 0;
                        for (var i = 0; i < itemCount; i++)
                        {
                            var price = rnd.Next(10, 200);
                            var qty = rnd.Next(1, 4);
                            total += price * qty;
                            orderItems.Add(new OrderItem
                            {
                                OrderId = 0,
                                ProductId = rnd.Next(1, 101),
                                VariantId = null,
                                Quantity = qty,
                                PriceAtTime = price,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            });
                        }

                        orders.Add(new Order
                        {
                            UserId = cart.UserId,
                            CartId = cart.Id,
                            Status = "paid",
                            PaymentStatus = "paid",
                            TotalAmount = total,
                            ShippingFee = 0,
                            DiscountAmount = 0,
                            FinalPrice = total,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                    }

                    await db.Orders.AddRangeAsync(orders, ct);
                    await Commit(db, ct);

                    var allOrders = await db.Orders.AsNoTracking().ToListAsync(ct);
                    var enumerator = allOrders.GetEnumerator();
                    foreach (var item in orderItems)
                    {
                        if (!enumerator.MoveNext())
                        {
                            enumerator = allOrders.GetEnumerator();
                            enumerator.MoveNext();
                        }
                        item.OrderId = enumerator.Current.Id;
                    }

                    await db.OrderItems.AddRangeAsync(orderItems, ct);
                    await Commit(db, ct);
                }, schema.EnableDiagnostics);

            await Section(inc, "Transactions",
                () => db.Transactions.AsNoTracking().AnyAsync(ct),
                async () =>
                {
                    var rnd = new Random();
                    var orders = await db.Orders.AsNoTracking().ToListAsync(ct);
                    var txs = orders.Select(o => new Transaction
                    {
                        OrderId = o.Id,
                        Amount = o.FinalPrice,
                        Method = "COD",
                        Status = "success",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }).ToList();
                    await db.Transactions.AddRangeAsync(txs, ct);
                    await Commit(db, ct);
                }, schema.EnableDiagnostics);

            await Section(inc, "OrderDeliveries",
                () => db.OrderDeliveries.AsNoTracking().AnyAsync(ct),
                async () =>
                {
                    var rnd = new Random();
                    var orders = await db.Orders.AsNoTracking().ToListAsync(ct);
                    var deliveries = orders.Select(o => new OrderDelivery
                    {
                        OrderId = o.Id,
                        DeliveryType = "standard",
                        DeliveryStatus = "delivered",
                        ShippingProvider = "MockExpress",
                        TrackingCode = $"TRK{o.Id:D6}",
                        ShippedAt = DateTime.UtcNow.AddDays(-rnd.Next(1, 5)),
                        DeliveredAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }).ToList();
                    await db.OrderDeliveries.AddRangeAsync(deliveries, ct);
                    await Commit(db, ct);
                }, schema.EnableDiagnostics);
        });
    }
    public static async Task SeedBlogTablesAsync(BlogSchema schema, CancellationToken ct = default)
    {
        await schema.WithSeedContextAsync(async () =>
        {
            var db = schema.Db;
            var inc = schema.Include;
            var now = DateTime.UtcNow;

            static async Task Section(
                HashSet<string>? include, string key,
                Func<Task<bool>> hasAnyAsync,
                Func<Task> seedAsync,
                bool diagnostics = false)
            {
                if (!SeedSchema<EcomBlogsContext>.ShouldInclude(include, key)) return;
                if (await hasAnyAsync()) return;
                if (!diagnostics)
                {
                    await seedAsync();
                    return;
                }
                var sw = Stopwatch.StartNew();
                await seedAsync();
                sw.Stop();
                Console.WriteLine($"[Seed][Blogs] {key} took {sw.ElapsedMilliseconds} ms");
            }

            static async Task Commit(DbContext db, CancellationToken ct)
            {
                db.ChangeTracker.DetectChanges();
                await db.SaveChangesAsync(false, ct);
                db.ChangeTracker.Clear();
            }

            await Section(inc, "Quotes",
                () => db.Quotes.AsNoTracking().AnyAsync(ct),
                async () =>
                {
                    for (var created = 0; created < schema.QuotesCount; created += schema.BatchSize)
                    {
                        var take = Math.Min(schema.BatchSize, schema.QuotesCount - created);
                        var batch = QuoteExtension.Quote.GetSeedData(take);
                        await db.Quotes.AddRangeAsync(batch, ct);
                        await Commit(db, ct);
                    }
                }, schema.EnableDiagnostics);
            var allQuotes = await db.Quotes.AsNoTracking().ToListAsync(ct);

            await Section(inc, "Tags",
                () => db.BlogTags.AsNoTracking().AnyAsync(ct),
                async () =>
                {
                    for (var created = 0; created < schema.TagsCount; created += schema.BatchSize)
                    {
                        var take = Math.Min(schema.BatchSize, schema.TagsCount - created);
                        var batch = BlogTagExtension.BlogTag.GetSeedData(take);
                        await db.BlogTags.AddRangeAsync(batch, ct);
                        await Commit(db, ct);
                    }
                }, schema.EnableDiagnostics);
            var allTags = await db.BlogTags.AsNoTracking().ToListAsync(ct);

            await Section(inc, "Blogs",
                () => db.Blogs.AsNoTracking().AnyAsync(ct),
                async () =>
                {
                    for (var created = 0; created < schema.BlogsCount; created += schema.BatchSize)
                    {
                        var take = Math.Min(schema.BatchSize, schema.BlogsCount - created);
                        var batch = BlogExtension.Blog.GetSeedData(take, allQuotes);
                        await db.Blogs.AddRangeAsync(batch, ct);
                        await Commit(db, ct);
                    }
                }, schema.EnableDiagnostics);
            var allBlogs = await db.Blogs.AsNoTracking().ToListAsync(ct);

            await Section(inc, "Categories",
                () => db.BlogCategories.AsNoTracking().AnyAsync(ct),
                async () =>
                {
                    if (allBlogs.Count == 0) return;

                    for (var created = 0; created < schema.CategoriesCount; created += schema.BatchSize)
                    {
                        var take = Math.Min(schema.BatchSize, schema.CategoriesCount - created);
                        var batch = BlogCategoryExtension.BlogCategory.GetSeedData(take, allBlogs);
                        await db.BlogCategories.AddRangeAsync(batch, ct);
                        await Commit(db, ct);
                    }
                }, schema.EnableDiagnostics);
            var allCategories = await db.BlogCategories.AsNoTracking().ToListAsync(ct);

            await Section(inc, "Variants",
                () => db.BlogVariants.AsNoTracking().AnyAsync(ct),
                async () =>
                {
                    foreach (var chunk in SeedSchema<EcomBlogsContext>.Chunk(allBlogs, schema.BatchSize))
                    {
                        var items = BlogVariantExtension.BlogVariant.GetSeedDataForBlogs(chunk, schema.MaxExtraVariantsPerBlog);
                        if (items.Count == 0) continue;
                        await db.BlogVariants.AddRangeAsync(items, ct);
                        await Commit(db, ct);
                    }
                }, schema.EnableDiagnostics);

            await Section(inc, "CategoryJoins",
                () => db.BlogCategoryJoins.AsNoTracking().AnyAsync(ct),
                async () =>
                {
                    foreach (var chunk in SeedSchema<EcomBlogsContext>.Chunk(allBlogs, schema.BatchSize))
                    {
                        var joins = new List<BlogCategoryJoin>();
                        foreach (var blog in chunk)
                        {
                            var picks = allCategories.OrderBy(_ => Guid.NewGuid()).Take(schema.CategoriesPerBlog).ToList();
                            foreach (var cat in picks)
                            {
                                joins.Add(new BlogCategoryJoin
                                {
                                    BlogId = blog.Id,
                                    BlogCategoryId = cat.Id,
                                    CreatedAt = now,
                                    UpdatedAt = now
                                });
                            }
                        }
                        if (joins.Count == 0) continue;
                        await db.BlogCategoryJoins.AddRangeAsync(joins, ct);
                        await Commit(db, ct);
                    }
                }, schema.EnableDiagnostics);

            await Section(inc, "TagJoins",
                () => db.BlogTagJoins.AsNoTracking().AnyAsync(ct),
                async () =>
                {
                    foreach (var chunk in SeedSchema<EcomBlogsContext>.Chunk(allBlogs, schema.BatchSize))
                    {
                        var joins = new List<BlogTagJoin>();
                        foreach (var blog in chunk)
                        {
                            var picks = allTags.OrderBy(_ => Guid.NewGuid()).Take(schema.TagsPerBlog).ToList();
                            foreach (var tag in picks)
                            {
                                joins.Add(new BlogTagJoin
                                {
                                    BlogId = blog.Id,
                                    BlogTagId = tag.Id,
                                    CreatedAt = now,
                                    UpdatedAt = now
                                });
                            }
                        }
                        if (joins.Count == 0) continue;
                        await db.BlogTagJoins.AddRangeAsync(joins, ct);
                        await Commit(db, ct);
                    }
                }, schema.EnableDiagnostics);

            await Section(inc, "Comments",
                () => db.BlogComments.AsNoTracking().AnyAsync(ct),
                async () =>
                {
                    foreach (var chunk in SeedSchema<EcomBlogsContext>.Chunk(allBlogs, schema.BatchSize))
                    {
                        var items = BlogCommentExtension.BlogComment.GetSeedDataForBlogs(chunk, schema.CommentsPerBlog);
                        if (items.Count == 0) continue;
                        await db.BlogComments.AddRangeAsync(items, ct);
                        await Commit(db, ct);
                    }
                }, schema.EnableDiagnostics);
        });
    }
}