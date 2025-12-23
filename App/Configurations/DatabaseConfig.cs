using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using App.DAL.BlogModels;
using App.DAL.UserModels;
using App.DAL.DataSeedings;
using App.UTIL.Abstractions.DAL;
using App.DAL.ProductModels;
using App.DAL.OrderModels;

namespace App.Configurations;

public static class DatabaseConfig
{
    /// <summary>
    /// Đăng ký DbContext với connection string từ configuration
    /// </summary>
    public static IServiceCollection ConfigurePersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var dbSection = configuration.GetSection("Database");
        
        // Register all DbContexts with their respective connection strings
        RegisterDbContext<EcomUsersContext>(services, configuration, dbSection, 
            "MyUserSqlConn", "UserSchema", "BatchSizeUsers", 1000);
        
        RegisterDbContext<EcomBlogsContext>(services, configuration, dbSection, 
            "MyBlogSqlConn", "BlogSchema", "BatchSizeBlogs", 1000);
        
        RegisterDbContext<EcomProductsContext>(services, configuration, dbSection, 
            "MyProductSqlConn", "ProductSchema", "BatchSizeProducts", 500);
        
        RegisterDbContext<EcomOrdersContext>(services, configuration, dbSection, 
            "MyOrderSqlConn", "OrderSchema", "BatchSizeOrders", 500);
        
        services.AddHostedService<DatabaseInitializerHostedService>();
        services.AddTransient<BlogSchema>();
        services.AddTransient<UserSchema>();
        services.AddTransient<ProductSchema>();
        services.AddTransient<OrderSchema>();
        return services;
    }

    /// <summary>
    /// Helper method to register a DbContext with proper connection string resolution.
    /// </summary>
    private static void RegisterDbContext<TContext>(
        IServiceCollection services,
        IConfiguration configuration,
        IConfigurationSection dbSection,
        string connStringName,
        string schemaSection,
        string batchSizeFallbackKey,
        int defaultBatchSize) where TContext : DbContext
    {
        services.AddDbContext<TContext>(options =>
        {
            // Simple connection string retrieval
            // ASP.NET Core automatically handles env vars mapped to ConnectionStrings
            var conn = configuration.GetConnectionString(connStringName);
            
            if (string.IsNullOrEmpty(conn))
            {
                throw new InvalidOperationException(
                    $"Connection string '{connStringName}' not found. " +
                    $"Check appsettings.json or 'ConnectionStrings__{connStringName}' env var.");
            }
            
            // Read batch size
            var maxBatch = dbSection.GetSection(schemaSection).GetValue<int?>("BatchSize")
                ?? dbSection.GetValue<int?>(batchSizeFallbackKey)
                ?? defaultBatchSize;
            
            options.UseMySql(conn, ServerVersion.AutoDetect(conn) ?? ServerVersion.Parse("8.4.0-mysql"), o =>
            {
                o.MaxBatchSize(maxBatch);
                o.EnableRetryOnFailure(
                    maxRetryCount: 10,
                    maxRetryDelay: TimeSpan.FromSeconds(60),
                    errorNumbersToAdd: null);
            })
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
            
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });
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

			var dbSection = config.GetSection("Database");
			var seedEnabled = dbSection.GetValue<bool>("SeedEnabled", false);
			var recreateOnStart = dbSection.GetValue<bool>("RecreateOnStart", false);
			var schemaMode = dbSection.GetValue<string>("SchemaMode")?.Trim() ?? "None";

            const int maxRetries = 10;
            for (var attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
					if (recreateOnStart)
                    {
                        _logger.LogWarning("Recreating database schema (users + blogs + products + orders)...");
                        var userDb = services.GetRequiredService<EcomUsersContext>();
                        var blogDb = services.GetRequiredService<EcomBlogsContext>();
                        var productDb = services.GetRequiredService<EcomProductsContext>();
                        var orderDb = services.GetRequiredService<EcomOrdersContext>();
                        userDb.Database.EnsureDeleted();
                        blogDb.Database.EnsureDeleted();
                        productDb.Database.EnsureDeleted();
                        orderDb.Database.EnsureDeleted();
                    }

					if (!string.Equals(schemaMode, "None", StringComparison.OrdinalIgnoreCase))
                    {
                        var userDb = services.GetRequiredService<EcomUsersContext>();
						// Users DB
						if (string.Equals(schemaMode, "Migrate", StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogInformation("Users DB: Applying migrations...");
                            userDb.Database.Migrate();
                        }
                        else
                        {
                            _logger.LogInformation("Users DB: Ensuring created from model...");
                            userDb.Database.EnsureCreated();
                        }

                        // Blogs DB
                        var blogDb = services.GetRequiredService<EcomBlogsContext>();
						if (string.Equals(schemaMode, "Migrate", StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogInformation("Blogs DB: Applying migrations...");
                            blogDb.Database.Migrate();
                        }
                        else
                        {
                            _logger.LogInformation("Blogs DB: Ensuring created from model...");
                            blogDb.Database.EnsureCreated();
                        }

                        // Products DB
                        var productDb = services.GetRequiredService<EcomProductsContext>();
                        if (string.Equals(schemaMode, "Migrate", StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogInformation("Products DB: Applying migrations...");
                            productDb.Database.Migrate();
                        }
                        else
                        {
                            _logger.LogInformation("Products DB: Ensuring created from model...");
                            productDb.Database.EnsureCreated();
                        }

                        // Orders DB
                        var ordersDb = services.GetRequiredService<EcomOrdersContext>();
                        if (string.Equals(schemaMode, "Migrate", StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogInformation("Orders DB: Applying migrations...");
                            ordersDb.Database.Migrate();
                        }
                        else
                        {
                            _logger.LogInformation("Orders DB: Ensuring created from model...");
                            ordersDb.Database.EnsureCreated();
                        }
                    }

					if (seedEnabled)
                    {
                        _logger.LogInformation("Seeding sample data via schemas...");
                        // Build include set from array or fallback string
                        var includeSection = dbSection.GetSection("SeedInclude");
                        HashSet<string>? includeSet = null;
                        if (includeSection.Exists() && includeSection.GetChildren().Any())
                        {
                            var arr = includeSection.Get<string[]>() ?? Array.Empty<string>();
                            includeSet = arr.Length == 0 ? null : new HashSet<string>(arr, StringComparer.OrdinalIgnoreCase);
                        }
                        else
                        {
                            var includeStr = dbSection.GetValue<string>("SeedInclude");
                            includeSet = string.IsNullOrWhiteSpace(includeStr) ? null
                                : new HashSet<string>(includeStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), StringComparer.OrdinalIgnoreCase);
                        }
                        var isDev = services.GetRequiredService<IHostEnvironment>().IsDevelopment();
                        var perBatchTransaction = dbSection.GetValue<bool>("PerBatchTransaction", true);

                        // Helper method to bind and run schema
                        async Task BindAndRunSchema<TContext, TSchema>(
                            string schemaKey,
                            string configSectionName,
                            Func<IServiceProvider, TContext> getContext,
                            Func<TContext, TSchema> createSchema)
                            where TContext : DbContext
                            where TSchema : SeedSchema<TContext>
                        {
                            if (includeSet != null && !includeSet.Contains(schemaKey))
                            {
                                _logger.LogInformation("Skipping {SchemaKey} schema (not in SeedInclude)", schemaKey);
                                return;
                            }

                            _logger.LogInformation("Seeding {SchemaKey} schema...", schemaKey);
                            try
                            {
                                var context = getContext(services);
                                var schema = createSchema(context);
                                var schemaSection = dbSection.GetSection(configSectionName);
                                
                                // Bind configuration to schema object
                                if (schemaSection.Exists() && schemaSection.GetChildren().Any())
                                {
                                    schemaSection.Bind(schema);
                                    _logger.LogInformation("{SchemaKey} schema configuration bound from {Section}", schemaKey, configSectionName);
                                }
                                else
                                {
                                    _logger.LogWarning("{Section} section not found in configuration, using defaults", configSectionName);
                                }
                                
                                // Set common properties
                                schema.PerBatchTransaction = perBatchTransaction;
                                schema.Include = includeSet;
                                schema.EnableDiagnostics = isDev;

                                await schema.RunAsync(cancellationToken);
                                _logger.LogInformation("{SchemaKey} schema seeding completed successfully", schemaKey);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error seeding {SchemaKey} schema", schemaKey);
                                throw;
                            }
                        }

                        // Seed Users Schema
                        await BindAndRunSchema<EcomUsersContext, UserSchema>(
                            "Users",
                            "UserSchema",
                            s => s.GetRequiredService<EcomUsersContext>(),
                            db => new UserSchema(db));

                        // Seed Blogs Schema
                        await BindAndRunSchema<EcomBlogsContext, BlogSchema>(
                            "Blogs",
                            "BlogSchema",
                            s => s.GetRequiredService<EcomBlogsContext>(),
                            db => new BlogSchema(db));

                        // Seed Products Schema
                        await BindAndRunSchema<EcomProductsContext, ProductSchema>(
                            "Products",
                            "ProductSchema",
                            s => s.GetRequiredService<EcomProductsContext>(),
                            db => new ProductSchema(db));

                        // Seed Orders Schema
                        await BindAndRunSchema<EcomOrdersContext, OrderSchema>(
                            "Orders",
                            "OrderSchema",
                            s => s.GetRequiredService<EcomOrdersContext>(),
                            db => new OrderSchema(db));
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
