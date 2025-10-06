using App.DAL.DataSeedings.ModelExtensions;
using App.DAL.UserModels;
using Microsoft.EntityFrameworkCore;
using App.DAL.BlogModels;
using App.DAL.DataSeedings.BlogExtensions;

namespace App.DAL.DataSeedings;

public static class ModelBuilderExtensions
{
    public static async Task SeedUserTablesAsync(
        EcomUsersContext context,
        int usersCount = 200,
        int addressBooksPerUser = 2,
        int auditLogsPerUser = 5,
        int cartsPerUser = 3,
        int contactsCount = 50,
        float newslettersProbabilityPerUser = 0.3f,
        int userTagsPerUser = 4,
        int wishlistsPerUser = 5,
        int batchSize = 1000,
        bool perBatchTransaction = true,
        CancellationToken ct = default)
    {
        static IEnumerable<List<T>> Chunk<T>(IReadOnlyList<T> source, int size)
        {
            for (var i = 0; i < source.Count; i += size)
                yield return source.Skip(i).Take(Math.Min(size, source.Count - i)).ToList();
        }

        var originalAutoDetect = context.ChangeTracker.AutoDetectChangesEnabled;
        var originalTracking = context.ChangeTracker.QueryTrackingBehavior;
        context.ChangeTracker.AutoDetectChangesEnabled = false;
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        try
        {
            if (!await context.Users.AsNoTracking().AnyAsync(ct))
            {
                for (var created = 0; created < usersCount; created += batchSize)
                {
                    var take = Math.Min(batchSize, usersCount - created);
                    var batch = UserExtension.User.GetSeedData(take);
                    await context.Users.AddRangeAsync(batch, ct);
                    context.ChangeTracker.DetectChanges();
                    await context.SaveChangesAsync(false, ct);
                    context.ChangeTracker.Clear();
                }
            }
            var usersAll = await context.Users.AsNoTracking().ToListAsync(ct);

            if (!await context.AddressBooks.AsNoTracking().AnyAsync(ct))
            {
                foreach (var userChunk in Chunk(usersAll, batchSize))
                {
                    var addressBooks = AddressBookExtension.AddressBook.GetSeedDataForUsers(userChunk, addressBooksPerUser);
                    if (addressBooks.Count > 0)
                    {
                        await context.AddressBooks.AddRangeAsync(addressBooks, ct);
                        context.ChangeTracker.DetectChanges();
                        await context.SaveChangesAsync(false, ct);
                        context.ChangeTracker.Clear();
                    }
                }
            }

            if (!await context.AuditLogs.AsNoTracking().AnyAsync(ct))
            {
                foreach (var userChunk in Chunk(usersAll, batchSize))
                {
                    var auditLogs = AuditLogExtension.AuditLog.GetSeedDataForUsers(userChunk, auditLogsPerUser);
                    if (auditLogs.Count > 0)
                    {
                        await context.AuditLogs.AddRangeAsync(auditLogs, ct);
                        context.ChangeTracker.DetectChanges();
                        await context.SaveChangesAsync(false, ct);
                        context.ChangeTracker.Clear();
                    }
                }
            }

            if (!await context.Carts.AsNoTracking().AnyAsync(ct))
            {
                foreach (var userChunk in Chunk(usersAll, batchSize))
                {
                    var carts = CartExtension.Cart.GetSeedDataForUsers(userChunk, cartsPerUser);
                    if (carts.Count > 0)
                    {
                        await context.Carts.AddRangeAsync(carts, ct);
                        context.ChangeTracker.DetectChanges();
                        await context.SaveChangesAsync(false, ct);
                        context.ChangeTracker.Clear();
                    }
                }
            }

            if (!await context.Contacts.AsNoTracking().AnyAsync(ct))
            {
                for (var created = 0; created < contactsCount; created += batchSize)
                {
                    var take = Math.Min(batchSize, contactsCount - created);
                    var contacts = ContactExtension.Contact.GetSeedData(take);
                    if (contacts.Count > 0)
                    {
                        await context.Contacts.AddRangeAsync(contacts, ct);
                        context.ChangeTracker.DetectChanges();
                        await context.SaveChangesAsync(false, ct);
                        context.ChangeTracker.Clear();
                    }
                }
            }

            if (!await context.Newsletters.AsNoTracking().AnyAsync(ct))
            {
                foreach (var userChunk in Chunk(usersAll, batchSize))
                {
                    var newsletters = NewsletterExtension.Newsletter.GetSeedDataForUsers(userChunk, newslettersProbabilityPerUser);
                    if (newsletters.Count > 0)
                    {
                        await context.Newsletters.AddRangeAsync(newsletters, ct);
                        context.ChangeTracker.DetectChanges();
                        await context.SaveChangesAsync(false, ct);
                        context.ChangeTracker.Clear();
                    }
                }
            }

            if (!await context.UserTags.AsNoTracking().AnyAsync(ct))
            {
                foreach (var userChunk in Chunk(usersAll, batchSize))
                {
                    var userTags = UserTagExtension.UserTag.GetSeedDataForUsers(userChunk, userTagsPerUser);
                    if (userTags.Count > 0)
                    {
                        await context.UserTags.AddRangeAsync(userTags, ct);
                        context.ChangeTracker.DetectChanges();
                        await context.SaveChangesAsync(false, ct);
                        context.ChangeTracker.Clear();
                    }
                }
            }

            if (!await context.Wishlists.AsNoTracking().AnyAsync(ct))
            {
                foreach (var userChunk in Chunk(usersAll, batchSize))
                {
                    var wishlists = WishlistExtension.Wishlist.GetSeedDataForUsers(userChunk, wishlistsPerUser);
                    if (wishlists.Count > 0)
                    {
                        await context.Wishlists.AddRangeAsync(wishlists, ct);
                        context.ChangeTracker.DetectChanges();
                        await context.SaveChangesAsync(false, ct);
                        context.ChangeTracker.Clear();
                    }
                }
            }

            // per-batch SaveChanges already commits (autocommit). No global transaction here
        }
        finally
        {
            context.ChangeTracker.AutoDetectChangesEnabled = originalAutoDetect;
            context.ChangeTracker.QueryTrackingBehavior = originalTracking;
        }
    }

    public static async Task SeedBlogTablesAsync(
        EcomBlogsContext context,
        int blogsCount = 50,
        int categoriesCount = 10,
        int tagsCount = 15,
        int commentsPerBlog = 5,
        int categoriesPerBlog = 2,
        int tagsPerBlog = 3,
        int quotesCount = 10,
        int batchSize = 1000,
        bool perBatchTransaction = true,
        CancellationToken ct = default)
    {
        static IEnumerable<List<T>> Chunk<T>(IReadOnlyList<T> source, int size)
        {
            for (var i = 0; i < source.Count; i += size)
                yield return source.Skip(i).Take(Math.Min(size, source.Count - i)).ToList();
        }

        var originalAutoDetect = context.ChangeTracker.AutoDetectChangesEnabled;
        var originalTracking = context.ChangeTracker.QueryTrackingBehavior;
        context.ChangeTracker.AutoDetectChangesEnabled = false;
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

        try
        {
            var now = DateTime.UtcNow;
            var random = new Random();

            if (!await context.Quotes.AsNoTracking().AnyAsync(ct))
            {
                for (var created = 0; created < quotesCount; created += batchSize)
                {
                    var take = Math.Min(batchSize, quotesCount - created);
                    var batch = QuoteExtension.Quote.GetSeedData(take);
                    await context.Quotes.AddRangeAsync(batch, ct);
                    context.ChangeTracker.DetectChanges();
                    await context.SaveChangesAsync(false, ct);
                    context.ChangeTracker.Clear();
                }
            }
            var allQuotes = await context.Quotes.AsNoTracking().ToListAsync(ct);

            if (!await context.BlogTags.AsNoTracking().AnyAsync(ct))
            {
                for (var created = 0; created < tagsCount; created += batchSize)
                {
                    var take = Math.Min(batchSize, tagsCount - created);
                    var batch = BlogTagExtension.BlogTag.GetSeedData(take);
                    await context.BlogTags.AddRangeAsync(batch, ct);
                    context.ChangeTracker.DetectChanges();
                    await context.SaveChangesAsync(false, ct);
                    context.ChangeTracker.Clear();
                }
            }
            var allTags = await context.BlogTags.AsNoTracking().ToListAsync(ct);

            if (!await context.BlogCategories.AsNoTracking().AnyAsync(ct))
            {
                for (var created = 0; created < categoriesCount; created += batchSize)
                {
                    var take = Math.Min(batchSize, categoriesCount - created);
                    var batch = BlogCategoryExtension.BlogCategory.GetSeedData(take);
                    await context.BlogCategories.AddRangeAsync(batch, ct);
                    context.ChangeTracker.DetectChanges();
                    await context.SaveChangesAsync(false, ct);
                    context.ChangeTracker.Clear();
                }
            }
            var allCategories = await context.BlogCategories.AsNoTracking().ToListAsync(ct);

            if (!await context.Blogs.AsNoTracking().AnyAsync(ct))
            {
                for (var created = 0; created < blogsCount; created += batchSize)
                {
                    var take = Math.Min(batchSize, blogsCount - created);
                    var batch = BlogExtension.Blog.GetSeedData(take, allQuotes);
                    await context.Blogs.AddRangeAsync(batch, ct);
                    context.ChangeTracker.DetectChanges();
                    await context.SaveChangesAsync(false, ct);
                    context.ChangeTracker.Clear();
                }
            }
            var allBlogs = await context.Blogs.AsNoTracking().ToListAsync(ct);

            if (!await context.BlogCategoryJoins.AsNoTracking().AnyAsync(ct) && allBlogs.Count > 0 && allCategories.Count > 0)
            {
                foreach (var blogChunk in Chunk(allBlogs, batchSize))
                {
                    var joins = new List<BlogCategoryJoin>();
                    foreach (var blog in blogChunk)
                    {
                        var picks = allCategories.OrderBy(_ => Guid.NewGuid()).Take(categoriesPerBlog).ToList();
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
                    if (joins.Count > 0)
                    {
                        await context.BlogCategoryJoins.AddRangeAsync(joins, ct);
                        context.ChangeTracker.DetectChanges();
                        await context.SaveChangesAsync(false, ct);
                        context.ChangeTracker.Clear();
                    }
                }
            }

            if (!await context.BlogTagJoins.AsNoTracking().AnyAsync(ct) && allBlogs.Count > 0 && allTags.Count > 0)
            {
                foreach (var blogChunk in Chunk(allBlogs, batchSize))
                {
                    var joins = new List<BlogTagJoin>();
                    foreach (var blog in blogChunk)
                    {
                        var picks = allTags.OrderBy(_ => Guid.NewGuid()).Take(tagsPerBlog).ToList();
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
                    if (joins.Count > 0)
                    {
                        await context.BlogTagJoins.AddRangeAsync(joins, ct);
                        context.ChangeTracker.DetectChanges();
                        await context.SaveChangesAsync(false, ct);
                        context.ChangeTracker.Clear();
                    }
                }
            }

            if (!await context.BlogComments.AsNoTracking().AnyAsync(ct) && allBlogs.Count > 0)
            {
                foreach (var blogChunk in Chunk(allBlogs, batchSize))
                {
                    var comments = BlogCommentExtension.BlogComment.GetSeedDataForBlogs(blogChunk, commentsPerBlog);
                    if (comments.Count > 0)
                    {
                        await context.BlogComments.AddRangeAsync(comments, ct);
                        context.ChangeTracker.DetectChanges();
                        await context.SaveChangesAsync(false, ct);
                        context.ChangeTracker.Clear();
                    }
                }
            }

            // per-batch SaveChanges already commits (autocommit). No global transaction here
        }
        finally
        {
            context.ChangeTracker.AutoDetectChangesEnabled = originalAutoDetect;
            context.ChangeTracker.QueryTrackingBehavior = originalTracking;
        }
    }
}