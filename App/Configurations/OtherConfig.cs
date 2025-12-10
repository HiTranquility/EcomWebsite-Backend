using System.Threading.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.ResponseCompression;
using App.UTIL.Abstractions.DTO.Response;

namespace App.Configurations;

public static class OtherConfig
{
    public static IServiceCollection ConfigureCors(this IServiceCollection services, IConfiguration configuration)
    {
        var corsSection = configuration.GetSection("Cors");
        var allowedOrigins = corsSection["AllowedOrigins"];
        var allowCredentials = corsSection.GetValue<bool?>("AllowCredentials") ?? false;

        services.AddCors(options =>
        {
            options.AddPolicy("CorsDevPolicy", builder =>
            {
                if (!string.IsNullOrWhiteSpace(allowedOrigins))
                {
                    var origins = allowedOrigins
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    builder.WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();

                    if (allowCredentials)
                        builder.AllowCredentials();
                }
                else
                {
                    // No configured origins: allow any origin but DO NOT allow credentials
                    builder.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
            });

            options.AddPolicy("CorsRestrictedPolicy", builder =>
            {
                if (!string.IsNullOrWhiteSpace(allowedOrigins))
                {
                    var origins = allowedOrigins
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    builder.WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();

                    if (allowCredentials)
                        builder.AllowCredentials();
                }
                else
                {
                    // Fallback: no origins configured → allow any origin without credentials
                    builder.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
            });
        });

        return services;
    }

    public static IServiceCollection ConfigureRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            options.AddPolicy("SignupPolicy", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1)
                    }));
        });
        return services;
    }

    public static IServiceCollection ConfigureApiBehavior(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var firstError = context.ModelState
                    .Where(kvp => (kvp.Value?.Errors?.Count ?? 0) > 0)
                    .SelectMany(kvp => kvp.Value?.Errors ?? Enumerable.Empty<ModelError>())
                    .Select(err =>
                    {
                        var message = err.ErrorMessage ?? string.Empty;
                        var parts = message.Split('|', 2);
                        var code = parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]) ? parts[0] : "INVALID";
                        var title = parts.Length > 1 ? (parts[1] ?? "Invalid input data") : (message ?? "Invalid input data");
                        return new { code, title };
                    })
                    .FirstOrDefault();

                var rsp = new BaseResponse();

                if (firstError != null)
                {
                    rsp.SetError(firstError.code, firstError.title, "Validation failed", 400);
                }
                else
                {
                    rsp.SetError("VALIDATION_ERROR", "Invalid input data", "Validation failed", 400);
                }

                return new ObjectResult(rsp) { StatusCode = 400 };
            };
        });
        return services;
    }

    public static IServiceCollection ConfigureResponseCompression(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });
        
        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = System.IO.Compression.CompressionLevel.Optimal;
        });
        
        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = System.IO.Compression.CompressionLevel.Optimal;
        });
        
        return services;
    }
}