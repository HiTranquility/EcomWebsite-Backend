using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using App.DAL.BlogModels;
using App.DAL.UserModels;
using App.DAL.DataSeedings;

namespace App.Configurations;

public static class DatabaseConfig
{
    private enum DbInitStrategy
    {
        Auto,
        Migrate,
        Model
    }

    /// <summary>
    /// Đăng ký DbContext với connection string từ configuration
    /// </summary>
    public static IServiceCollection ConfigurePersistence(this IServiceCollection services, IConfiguration configuration)
    {
        //Tương lai, nếu chỉ có 1 cái thi xoa bot
        services.AddDbContext<EcomUsersContext>(options =>
        {
            var conn = configuration.GetConnectionString("MyUserSqlConn");
            var maxBatch = configuration.GetSection("Database").GetValue<int>("BatchSizeUsers", 1000);
            options.UseMySql(conn, ServerVersion.AutoDetect(conn), o => o.MaxBatchSize(maxBatch))
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });
        services.AddDbContext<EcomBlogsContext>(options =>
        {
            var conn = configuration.GetConnectionString("MyBlogSqlConn");
            var maxBatch = configuration.GetSection("Database").GetValue<int>("BatchSizeBlogs", 1000);
            options.UseMySql(conn, ServerVersion.AutoDetect(conn), o => o.MaxBatchSize(maxBatch))
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });
        services.AddHostedService<DatabaseInitializerHostedService>();
        return services;
    }

    private sealed class DatabaseInitializerHostedService : IHostedService
    {   
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DatabaseInitializerHostedService> _logger;

        public DatabaseInitializerHostedService(IServiceScopeFactory scopeFactory, ILogger<DatabaseInitializerHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var services = scope.ServiceProvider;

            var config = services.GetRequiredService<IConfiguration>();
            var envHost = services.GetRequiredService<IHostEnvironment>();
            var db = services.GetRequiredService<EcomUsersContext>();
            var blogDb = services.GetRequiredService<EcomBlogsContext>();

            var dbSection = config.GetSection("Database");
            var defaultRecreate = envHost.IsDevelopment();
            var autoMigrate = dbSection.GetValue<bool>("AutoMigrate", true);
            var autoSeed = dbSection.GetValue<bool>("AutoSeed", false);
            var recreate = dbSection.GetValue<bool>("Recreate", defaultRecreate);

            var defaultStrategyName = envHost.IsDevelopment() ? nameof(DbInitStrategy.Model) : nameof(DbInitStrategy.Migrate);
            var strategyStr = dbSection.GetValue<string>("Strategy", defaultStrategyName);
            Enum.TryParse<DbInitStrategy>(strategyStr, ignoreCase: true, out var strategy);

            const int maxRetries = 10;
            for (var attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    if (recreate)
                    {
                        _logger.LogWarning("Recreating database schema (users + blogs)...");
                        db.Database.EnsureDeleted();
                        blogDb.Database.EnsureDeleted();
                    }

                    if (autoMigrate)
                    {
                        // Users DB
                        switch (strategy)
                        {
                            case DbInitStrategy.Migrate:
                                _logger.LogInformation("Users DB: Applying migrations...");
                                db.Database.Migrate();
                                break;
                            case DbInitStrategy.Model:
                                _logger.LogInformation("Users DB: Ensuring created from model...");
                                db.Database.EnsureCreated();
                                break;
                            default:
                                var hasMigrationsUsers = db.Database.GetMigrations().Any();
                                if (hasMigrationsUsers)
                                {
                                    _logger.LogInformation("Users DB: Migrations found. Applying...");
                                    db.Database.Migrate();
                                }
                                else
                                {
                                    _logger.LogInformation("Users DB: No migrations. Ensuring created from model...");
                                    db.Database.EnsureCreated();
                                }
                                break;
                        }

                        // Blogs DB
                        switch (strategy)
                        {
                            case DbInitStrategy.Migrate:
                                _logger.LogInformation("Blogs DB: Applying migrations...");
                                blogDb.Database.Migrate();
                                break;
                            case DbInitStrategy.Model:
                                _logger.LogInformation("Blogs DB: Ensuring created from model...");
                                blogDb.Database.EnsureCreated();
                                break;
                            default:
                                var hasMigrationsBlogs = blogDb.Database.GetMigrations().Any();
                                if (hasMigrationsBlogs)
                                {
                                    _logger.LogInformation("Blogs DB: Migrations found. Applying...");
                                    blogDb.Database.Migrate();
                                }
                                else
                                {
                                    _logger.LogInformation("Blogs DB: No migrations. Ensuring created from model...");
                                    blogDb.Database.EnsureCreated();
                                }
                                break;
                        }
                    }

                    if (autoSeed)
                    {
                        _logger.LogInformation("Seeding sample data via ModelBuilderExtensions...");
                        var include = dbSection.GetValue<string>("SeedInclude");
                        if (string.IsNullOrWhiteSpace(include) || include.Contains("Users", StringComparison.OrdinalIgnoreCase))
                        {
                        var usersCount = dbSection.GetValue<int>("UsersCount", 200);
                            var abPerUser = dbSection.GetValue<int>("AddressBooksPerUser", 2);
                            var auditPerUser = dbSection.GetValue<int>("AuditLogsPerUser", 5);
                            var cartsPerUser = dbSection.GetValue<int>("CartsPerUser", 3);
                            var contactsCount = dbSection.GetValue<int>("ContactsCount", 50);
                            var newsletterP = dbSection.GetValue<float>("NewslettersProbabilityPerUser", 0.3f);
                            var tagsPerUser = dbSection.GetValue<int>("UserTagsPerUser", 4);
                            var wishlistsPerUser = dbSection.GetValue<int>("WishlistsPerUser", 5);
                        var batchSizeUsers = dbSection.GetValue<int>("BatchSizeUsers", 1000);
                        var perBatchTx = dbSection.GetValue<bool>("PerBatchTransaction", true);

                            await ModelBuilderExtensions.SeedUserTablesAsync(
                                db,
                                usersCount,
                                abPerUser,
                                auditPerUser,
                                cartsPerUser,
                                contactsCount,
                                newsletterP,
                                tagsPerUser,
                                wishlistsPerUser,
                                batchSizeUsers,
                                perBatchTx,
                                cancellationToken
                            );
                        }

                        if (string.IsNullOrWhiteSpace(include) || include.Contains("Blogs", StringComparison.OrdinalIgnoreCase))
                        {
                            var blogsCount = dbSection.GetValue<int>("BlogsCount", 50);
                            var blogCategoriesCount = dbSection.GetValue<int>("BlogCategoriesCount", 10);
                            var blogTagsCount = dbSection.GetValue<int>("BlogTagsCount", 15);
                            var commentsPerBlog = dbSection.GetValue<int>("CommentsPerBlog", 5);
                            var categoriesPerBlog = dbSection.GetValue<int>("CategoriesPerBlog", 2);
                            var tagsPerBlog = dbSection.GetValue<int>("TagsPerBlog", 3);
                            var quotesCount = dbSection.GetValue<int>("QuotesCount", 10);
                            var batchSizeBlogs = dbSection.GetValue<int>("BatchSizeBlogs", 1000);
                            var perBatchTx = dbSection.GetValue<bool>("PerBatchTransaction", true);

                            await ModelBuilderExtensions.SeedBlogTablesAsync(
                                blogDb,
                                blogsCount,
                                blogCategoriesCount,
                                blogTagsCount,
                                commentsPerBlog,
                                categoriesPerBlog,
                                tagsPerBlog,
                                quotesCount,
                                batchSizeBlogs,
                                perBatchTx,
                                cancellationToken
                            );
                        }
                    }
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "DB init attempt {Attempt}/{Max} failed. Retrying...", attempt, maxRetries);
                    if (attempt == maxRetries) throw;
                    await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
                }
            }
        }
    }

}
