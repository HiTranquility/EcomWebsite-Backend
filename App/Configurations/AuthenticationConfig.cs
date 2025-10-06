using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using App.UTIL.Abstractions.DTO.Response;
namespace App.Configurations;

public static class AuthenticationConfig
{
    public static IServiceCollection ConfigureJwt(this IServiceCollection services, IConfiguration configuration)
    {
        var issuer = configuration["JwtSettings:Issuer"] ?? throw new InvalidOperationException("Missing JwtSettings:Issuer");
        var audience = configuration["JwtSettings:Audience"] ?? throw new InvalidOperationException("Missing JwtSettings:Audience");
        var secret = configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("Missing JwtSettings:SecretKey");

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        // 1️⃣ Đăng ký Authentication với JWT Bearer
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        var rsp = new BaseResponse();
                        rsp.SetError("UNAUTHORIZED", "Invalid or missing token", "Unauthorized", 401);
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsJsonAsync(rsp);
                    },
                    OnForbidden = context =>
                    {
                        var rsp = new BaseResponse();
                        rsp.SetError("FORBIDDEN", "Forbidden", "Forbidden", 403);
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";
                        return context.Response.WriteAsJsonAsync(rsp);
                    }
                };
            });

        return services;
    }

    public static IServiceCollection ConfigureGoogle(this IServiceCollection services, IConfiguration configuration)
    {
        throw new NotImplementedException();
    }

    public static IServiceCollection ConfigureFacebook(this IServiceCollection services, IConfiguration configuration)
    {
        throw new NotImplementedException();
    }

    public static IServiceCollection ConfigureApikey(this IServiceCollection services, IConfiguration configuration)
    {
        throw new NotImplementedException();
    }
}