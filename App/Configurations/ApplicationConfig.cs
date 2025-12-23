using App.BLL.Behaviors;
using App.BLL.Interfaces;
using App.BLL.Mappers;
using App.BLL.Services;
using App.BLL.Validators;
using App.DAL.Interfaces;
using App.DAL.Repositories;
using App.INFRA.Caching;
using App.INFRA.Identity;
using App.INFRA.Logging;
using App.INFRA.Email;
using App.INFRA.ExternalAuth.Google;
using App.INFRA.ExternalAuth.Facebook;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace App.Configurations;

public static class ApplicationConfig
{
    public static IServiceCollection ConfigureService(this IServiceCollection services, IConfiguration configuration)
    {
        #region --- Repositories (Interface + Concrete registration) ---
        
        // User Repositories
        services.AddScoped<UserRepo>();
        services.AddScoped<IUserRepo>(sp => sp.GetRequiredService<UserRepo>());
        services.AddScoped<RoleRepo>();
        services.AddScoped<IRoleRepo>(sp => sp.GetRequiredService<RoleRepo>());
        services.AddScoped<RefreshTokenRepo>();
        services.AddScoped<IRefreshTokenRepo>(sp => sp.GetRequiredService<RefreshTokenRepo>());
        services.AddScoped<SocialAccountRepo>();
        services.AddScoped<ISocialAccountRepo>(sp => sp.GetRequiredService<SocialAccountRepo>());
        services.AddScoped<AddressBookRepo>();
        services.AddScoped<IAddressBookRepo>(sp => sp.GetRequiredService<AddressBookRepo>());
        services.AddScoped<WishlistRepo>();
        services.AddScoped<IWishlistRepo>(sp => sp.GetRequiredService<WishlistRepo>());
        services.AddScoped<UserTagRepo>();
        services.AddScoped<IUserTagRepo>(sp => sp.GetRequiredService<UserTagRepo>());

        // Blog Repositories
        services.AddScoped<BlogRepo>();
        services.AddScoped<IBlogRepo>(sp => sp.GetRequiredService<BlogRepo>());
        services.AddScoped<BlogCommentRepo>();
        services.AddScoped<IBlogCommentRepo>(sp => sp.GetRequiredService<BlogCommentRepo>());

        // Product Repositories
        services.AddScoped<ProductRepo>();
        services.AddScoped<IProductRepo>(sp => sp.GetRequiredService<ProductRepo>());
        services.AddScoped<ManufacturerRepo>();
        services.AddScoped<IManufacturerRepo>(sp => sp.GetRequiredService<ManufacturerRepo>());
        services.AddScoped<ProductCategoryRepo>();
        services.AddScoped<IProductCategoryRepo>(sp => sp.GetRequiredService<ProductCategoryRepo>());
        services.AddScoped<ProductReviewRepo>();
        services.AddScoped<IProductReviewRepo>(sp => sp.GetRequiredService<ProductReviewRepo>());

        // Order Repositories
        services.AddScoped<CartRepo>();
        services.AddScoped<ICartRepo>(sp => sp.GetRequiredService<CartRepo>());
        services.AddScoped<OrderRepo>();
        services.AddScoped<IOrderRepo>(sp => sp.GetRequiredService<OrderRepo>());
        services.AddScoped<OrderItemRepo>();
        services.AddScoped<IOrderItemRepo>(sp => sp.GetRequiredService<OrderItemRepo>());
        services.AddScoped<TransactionRepo>();
        services.AddScoped<ITransactionRepo>(sp => sp.GetRequiredService<TransactionRepo>());
        services.AddScoped<PaymentsLogRepo>();
        services.AddScoped<IPaymentsLogRepo>(sp => sp.GetRequiredService<PaymentsLogRepo>());
        services.AddScoped<OrderDeliveryRepo>();
        services.AddScoped<IOrderDeliveryRepo>(sp => sp.GetRequiredService<OrderDeliveryRepo>());
        
        #endregion

        #region --- Services (Interface-based DI) ---
        
        // Auth & User Services
        services.AddScoped<IAuthSvc, AuthSvc>();
        services.AddScoped<IUserSvc, UserSvc>();
        services.AddScoped<IAddressBookSvc, AddressBookSvc>();

        // Blog Services
        services.AddScoped<IBlogSvc, BlogSvc>();
        services.AddScoped<IBlogCommentSvc, BlogCommentSvc>();

        // Product Services
        services.AddScoped<IProductSvc, ProductSvc>();
        services.AddScoped<IProductReviewSvc, ProductReviewSvc>();
        services.AddScoped<IProductTagSvc, ProductTagSvc>();
        services.AddScoped<IWishlistSvc, WishlistSvc>();

        // Order & Payment Services
        services.AddScoped<ICartSvc, CartSvc>();
        services.AddScoped<IOrderSvc, OrderSvc>();
        services.AddScoped<IPaymentSvc, PaymentSvc>();
        services.AddScoped<IOrderDeliverySvc, OrderDeliverySvc>();
        
        // Payment Provider Services
        services.AddScoped<IStripePaymentSvc, StripePaymentSvc>();
        services.AddScoped<IMoMoPaymentSvc, MoMoPaymentSvc>();
        services.AddScoped<IVNPayPaymentSvc, VNPayPaymentSvc>();
        
        #endregion

        #region --- Infrastructure Services ---
        
        services.AddMemoryCache();

        // Caching
        services.AddSingleton<ICacheService, HybridCacheService>();
        
        // Identity/Token
        services.AddSingleton<ITokenService, JwtTokenService>();
        
        // Messaging (RabbitMQ/Kafka) - now configured in InfrastructureConfig.ConfigureMessaging()
        // which handles conditional registration based on Enabled flags

        // Email
        // services.AddSingleton<IEmailService, SmtpEmailService>();
        
        // External Auth - Google
        services.AddHttpClient<IGoogleService, GoogleService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.BaseAddress = new Uri("https://oauth2.googleapis.com");
        });
        
        // External Auth - Facebook (disabled - not configured)
        // Register null so DI can resolve IFacebookService (injects as null into AuthSvc)
        services.AddSingleton<IFacebookService>(sp => null!);
        
        #endregion

        #region --- Logging & Mapping ---
        
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddConsole();
            loggingBuilder.AddDebug();
        });

        services.AddSingleton<IMapper>(sp =>
        {
            ILoggerFactory loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            MapperConfiguration mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AuthProfile>();
                cfg.AddProfile<BlogProfile>();
                cfg.AddProfile<BlogCommentProfile>();
                cfg.AddProfile<ProductProfile>();
                cfg.AddProfile<OrderProfile>();
                cfg.AddProfile<WishlistProfile>();
                cfg.AddProfile<PaymentProfile>();
                cfg.AddProfile<CartProfile>();
                cfg.AddProfile<OrderDeliveryProfile>();
                cfg.AddProfile<UserProfile>();
            }, loggerFactory);
            return mapperConfig.CreateMapper();
        });
        
        #endregion

        services.AddHttpContextAccessor();

        return services;
    }
    
    #region --- MediatR (CQRS Pattern) ---
    
    /// <summary>
    /// Configure MediatR with handlers and pipeline behaviors.
    /// </summary>
    public static IServiceCollection ConfigureMediatR(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<ProductSvc>();
            
            // Pipeline behaviors in order: Logging → Validation → Peáp drformance
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        });
        
        return services;
    }
    
    #endregion
    
    #region --- Audit Logging ---
    
    /// <summary>
    /// Configure audit logging services.
    /// </summary>
    public static IServiceCollection ConfigureAuditLogging(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IAuditLogger, AuditLogger>();
        return services;
    }
    
    #endregion
}
