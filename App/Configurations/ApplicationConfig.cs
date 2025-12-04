using App.BLL.Mappers;
using App.BLL.Services;
using App.BLL.Validators;
using App.DAL.Repositories;
using App.UTIL.Helpers.Cache;
using App.UTIL.Helpers.Google;
using App.UTIL.Helpers.Facebook;
using App.UTIL.Helpers.Token;
using App.UTIL.Helpers.Email;
using App.UTIL.Helpers.Message;
using App.UTIL.Helpers.Message.Schemas;
using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Configuration;

namespace App.Configurations;

public static class ApplicationConfig
{
    public static IServiceCollection ConfigureService(this IServiceCollection services, IConfiguration configuration)
    {
        // User Services
        services.AddScoped<UserRepo>();
        services.AddScoped<RoleRepo>();
        services.AddScoped<RefreshTokenRepo>();
        services.AddScoped<SocialAccountRepo>();
        services.AddScoped<AddressBookRepo>();
        services.AddScoped<WishlistRepo>();
        services.AddScoped<UserTagRepo>(); // Added UserTagRepo
        services.AddScoped<AuthSvc>();
        services.AddScoped<UserSvc>();
        services.AddScoped<AddressBookSvc>();
        

        // Blog Services
        services.AddScoped<BlogRepo>();
        services.AddScoped<BlogSvc>();
        services.AddScoped<BlogCommentRepo>();
        services.AddScoped<BlogCommentSvc>();

        // Product Services
        services.AddScoped<ProductRepo>();
        services.AddScoped<ManufacturerRepo>();
        services.AddScoped<ProductCategoryRepo>();
        services.AddScoped<ProductReviewRepo>();
        services.AddScoped<ProductSvc>();
        services.AddScoped<ProductReviewSvc>();
        services.AddScoped<ProductTagSvc>(); // Added ProductTagSvc
        services.AddScoped<WishlistSvc>();

        // Order Services
        services.AddScoped<CartRepo>();
        services.AddScoped<OrderRepo>();
        services.AddScoped<OrderItemRepo>();
        services.AddScoped<TransactionRepo>();
        services.AddScoped<PaymentsLogRepo>();
        services.AddScoped<OrderDeliveryRepo>();
        services.AddScoped<CartSvc>();
        services.AddScoped<OrderSvc>();
        services.AddScoped<PaymentSvc>();
        services.AddScoped<OrderDeliverySvc>();

        services.AddMemoryCache();

        services.AddSingleton<ICacheService, HybridCacheService>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        
        services.AddSingleton<IKafkaService, KafkaService>();
        services.AddSingleton<IEventPublisher, KafkaPublisher>();
        services.AddSingleton<IEventConsumer, KafkaConsumer>();

        services.AddSingleton<IEventBrokerConnection, RabbitMqConnection>();
        services.AddSingleton<IEventPublisher, RabbitMqPublisher>();
        services.AddSingleton<IEventConsumer, RabbitMqConsumer>();


//        services.AddSingleton<IEmailService, QueueEmailService>();
        services.AddHttpClient<IGoogleService, GoogleService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.BaseAddress = new Uri("https://oauth2.googleapis.com");
        });
        services.AddHttpClient<IFacebookService, FacebookService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.BaseAddress = new Uri("https://graph.facebook.com");
        });
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

        services.AddHttpContextAccessor();

        // FluentValidation validators are registered in Program.cs via AddFluentValidation
        // No need to register here if using automatic validation

        return services;
    }
}