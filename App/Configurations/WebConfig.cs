using System;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.CookiePolicy;
using FluentValidation;
using FluentValidation.AspNetCore;
using App.UTIL.Abstractions.DTO.Response;
using App.BLL.Validators.Auth;
using Microsoft.AspNetCore.RateLimiting;

namespace App.Configurations;

/// <summary>
/// Consolidated Web configuration.
/// Includes: CORS, RateLimiting, Compression, Validation, Session, Response Caching.
/// </summary>
public static class WebConfig
{
    /// <summary>
    /// Configure all web-related services in one call.
    /// </summary>
    public static IServiceCollection ConfigureWeb(this IServiceCollection services, IConfiguration configuration)
    {
        // CORS
        ConfigureCorsInternal(services, configuration);
        
        // Rate Limiting
        ConfigureRateLimitingInternal(services, configuration);
        
        // Response Compression
        ConfigureResponseCompressionInternal(services);
        
        // FluentValidation
        ConfigureFluentValidationInternal(services);
        
        // API Behavior (ModelState handling)
        ConfigureApiBehaviorInternal(services);
        
        // Response Caching
        ConfigureResponseCachingInternal(services, configuration);
        
        // Session (optional - disabled by default)
        // ConfigureSessionInternal(services, configuration);
        
        return services;
    }

    #region --- CORS ---
    
    private static void ConfigureCorsInternal(IServiceCollection services, IConfiguration configuration)
    {
        var corsSection = configuration.GetSection("Cors");
        
        // Read AllowedOrigins as comma-separated string and split
        var allowedOriginsString = corsSection.GetValue<string>("AllowedOrigins") ?? "";
        var allowedOrigins = allowedOriginsString
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .ToArray();
        
        // Fallback for dev if no config
        var devOrigins = allowedOrigins.Length > 0 
            ? allowedOrigins 
            : new[] { "http://localhost:5173", "http://localhost:3000", "http://localhost:4000" };
        
        // Fallback for prod/staging if no config  
        var prodOrigins = allowedOrigins.Length > 0 
            ? allowedOrigins 
            : new[] { "https://yourdomain.com" };

        services.AddCors(options =>
        {
            // Development policy - permissive
            options.AddPolicy("CorsDevPolicy", policy =>
            {
                policy.WithOrigins(devOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials()
                      .WithExposedHeaders("Content-Disposition", "X-Rate-Limit-Limit", "X-Rate-Limit-Remaining", "X-Rate-Limit-Reset");
            });

            // Production/Staging policy - restrictive but using configured origins
            options.AddPolicy("CorsRestrictedPolicy", policy =>
            {
                policy.WithOrigins(prodOrigins)
                      .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS", "PATCH")
                      .WithHeaders("Authorization", "Content-Type", "Accept", "X-Requested-With", "X-Api-Key", "X-Correlation-Id")
                      .AllowCredentials()
                      .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
            });
        });
    }
    
    #endregion

    #region --- Rate Limiting ---
    
    private static void ConfigureRateLimitingInternal(IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("RateLimiting");
        var enabled = section.GetValue<bool?>("Enabled") ?? true;
        
        if (!enabled) return;

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";
                
                var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
                    ? retryAfterValue.TotalSeconds : 60;
                    
                context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter).ToString();
                
                var response = new BaseResponse();
                response.SetError("RATE_LIMIT", "Too Many Requests", "Rate limit exceeded. Please try again later.", 429);
                await context.HttpContext.Response.WriteAsJsonAsync(response, token);
            };
            
            // Standard API policy
            options.AddFixedWindowLimiter("StandardApi", opt =>
            {
                opt.PermitLimit = section.GetValue<int?>("StandardApi:PermitLimit") ?? 100;
                opt.Window = TimeSpan.FromMinutes(section.GetValue<int?>("StandardApi:WindowMinutes") ?? 1);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 10;
            });
            
            // Auth endpoints - stricter
            options.AddSlidingWindowLimiter("AuthEndpoints", opt =>
            {
                opt.PermitLimit = section.GetValue<int?>("AuthEndpoints:PermitLimit") ?? 10;
                opt.Window = TimeSpan.FromMinutes(section.GetValue<int?>("AuthEndpoints:WindowMinutes") ?? 15);
                opt.SegmentsPerWindow = 3;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 2;
            });
            
            // Search endpoints
            options.AddTokenBucketLimiter("SearchEndpoints", opt =>
            {
                opt.TokenLimit = 50;
                opt.TokensPerPeriod = 10;
                opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
                opt.QueueLimit = 5;
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
        });
    }
    
    #endregion

    #region --- Response Compression ---
    
    private static void ConfigureResponseCompressionInternal(IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
            {
                "application/json",
                "text/json",
                "text/plain",
                "text/html",
                "text/css",
                "application/javascript",
                "text/javascript",
                "image/svg+xml"
            });
        });

        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = System.IO.Compression.CompressionLevel.Fastest;
        });

        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = System.IO.Compression.CompressionLevel.SmallestSize;
        });
    }
    
    #endregion

    #region --- FluentValidation ---
    
    private static void ConfigureFluentValidationInternal(IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation(config =>
        {
            config.DisableDataAnnotationsValidation = true;
        });
        
        services.AddValidatorsFromAssemblyContaining<LoginReqValidator>(ServiceLifetime.Scoped);
    }
    
    #endregion

    #region --- API Behavior ---
    
    private static void ConfigureApiBehaviorInternal(IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = false;
            
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? []
                    );

                var response = new BaseResponse();
                response.SetError("VALIDATION_ERROR", "Validation Failed", "One or more validation errors occurred.", 400);
                response.Payload = errors;
                
                return new BadRequestObjectResult(response);
            };
        });
    }
    
    #endregion

    #region --- Response Caching ---
    
    private static void ConfigureResponseCachingInternal(IServiceCollection services, IConfiguration configuration)
    {
        services.AddResponseCaching(options =>
        {
            options.MaximumBodySize = 100 * 1024 * 1024; // 100 MB
            options.SizeLimit = 1024 * 1024 * 1024; // 1 GB
            options.UseCaseSensitivePaths = false;
        });
    }
    
    #endregion

    #region --- Session (Optional) ---
    
    public static IServiceCollection ConfigureSession(this IServiceCollection services, IConfiguration configuration)
    {
        var sessionSection = configuration.GetSection("Session");
        var idleTimeoutMinutes = sessionSection.GetValue<int?>("IdleTimeoutMinutes") ?? 30;
        var cookieName = sessionSection.GetValue<string>("CookieName") ?? ".EcomWebsite.Session";
        var sameSite = sessionSection.GetValue<string>("SameSite") ?? "None";
        var securePolicySetting = sessionSection.GetValue<string>("SecurePolicy") ?? "Always";
        var isEssential = sessionSection.GetValue<bool?>("IsEssential") ?? true;

        var sameSiteMode = sameSite.ToUpperInvariant() switch
        {
            "STRICT" => SameSiteMode.Strict,
            "LAX" => SameSiteMode.Lax,
            _ => SameSiteMode.None
        };

        var securePolicy = securePolicySetting.ToUpperInvariant() switch
        {
            "ALWAYS" => CookieSecurePolicy.Always,
            "NONE" => CookieSecurePolicy.None,
            _ => CookieSecurePolicy.SameAsRequest
        };

        services.AddDistributedMemoryCache();

        services.Configure<CookiePolicyOptions>(options =>
        {
            options.MinimumSameSitePolicy = sameSiteMode;
            options.HttpOnly = HttpOnlyPolicy.Always;
            options.Secure = securePolicy;
        });

        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(idleTimeoutMinutes);
            options.Cookie.Name = cookieName;
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = isEssential;
            options.Cookie.SameSite = sameSiteMode;
            options.Cookie.SecurePolicy = securePolicy;
        });

        return services;
    }
    
    #endregion

    #region --- Middleware Extensions ---
    
    /// <summary>
    /// Use response caching middleware in the pipeline.
    /// </summary>
    public static WebApplication UseResponseCachingMiddleware(this WebApplication app)
    {
        app.UseResponseCaching();
        return app;
    }
    
    #endregion
}

/// <summary>
/// Cache profile definitions for controllers.
/// </summary>
public static class CacheProfilesConfig
{
    public static IMvcBuilder ConfigureCacheProfiles(this IMvcBuilder builder)
    {
        builder.AddMvcOptions(options =>
        {
            options.CacheProfiles.Add("Static", new CacheProfile
            {
                Duration = 3600,
                Location = ResponseCacheLocation.Any,
                VaryByHeader = "Accept-Encoding"
            });
            
            options.CacheProfiles.Add("ProductList", new CacheProfile
            {
                Duration = 300,
                Location = ResponseCacheLocation.Any,
                VaryByQueryKeys = ["page", "pageSize", "category", "manufacturer", "minPrice", "maxPrice", "sort", "keyword"]
            });
            
            options.CacheProfiles.Add("ProductDetail", new CacheProfile
            {
                Duration = 600,
                Location = ResponseCacheLocation.Any,
                VaryByHeader = "Accept-Encoding"
            });
            
            options.CacheProfiles.Add("BlogList", new CacheProfile
            {
                Duration = 600,
                Location = ResponseCacheLocation.Any,
                VaryByQueryKeys = ["page", "pageSize", "category", "tag"]
            });
            
            options.CacheProfiles.Add("Filters", new CacheProfile
            {
                Duration = 1800,
                Location = ResponseCacheLocation.Any
            });
            
            options.CacheProfiles.Add("Short", new CacheProfile
            {
                Duration = 60,
                Location = ResponseCacheLocation.Any
            });
            
            options.CacheProfiles.Add("NoCache", new CacheProfile
            {
                Duration = 0,
                Location = ResponseCacheLocation.None,
                NoStore = true
            });
        });
        
        return builder;
    }
}
