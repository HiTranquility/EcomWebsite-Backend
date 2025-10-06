using App.BLL.Mappers;
using App.BLL.Services;
using App.DAL.Repositories;
using App.UTIL.Helpers.Cache;
using App.UTIL.Helpers.Jwt;
using AutoMapper;

namespace App.Configurations;

public static class ApplicationConfig
{
    public static IServiceCollection ConfigureService(this IServiceCollection services)
    {
        // User Services
        services.AddScoped<UserRepo>();
        services.AddScoped<AuthSvc>();
        
        // Blog Services
        services.AddScoped<BlogRepo>();
        services.AddScoped<BlogSvc>();
        
        services.AddSingleton<ICacheService, HybridCacheAdapter>();
        services.AddSingleton<IJwtService, JwtAdapter>(); 
        
        services.AddSingleton<IMapper>(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AuthProfile>();
                cfg.AddProfile<BlogProfile>();
            }, loggerFactory);
            return mapperConfig.CreateMapper();
        });
        return services;
    }
}