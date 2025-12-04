using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using App.UTIL.Abstractions.DTO.Response;
namespace App.Configurations;

public static class AuthenticationConfig
{
    public static IServiceCollection ConfigureJwt(this IServiceCollection services, IConfiguration configuration)
    {
        var issuer = configuration["JwtSettings:Issuer"] ??
                     throw new InvalidOperationException("Missing JwtSettings:Issuer");
        var audience = configuration["JwtSettings:Audience"] ??
                       throw new InvalidOperationException("Missing JwtSettings:Audience");
        var secret = configuration["JwtSettings:SecretKey"] ??
                     throw new InvalidOperationException("Missing JwtSettings:SecretKey");

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
        // Note: This config is optional. Our app primarily uses JWT + manual Google code/id_token exchange.
        // If you enable this, do NOT override default schemes already set by JWT.
        var googleSection = configuration.GetSection("GoogleAuth");
        var clientId = googleSection["ClientId"] ?? throw new InvalidOperationException("Missing GoogleAuth:ClientId");
        var clientSecret = googleSection["ClientSecret"] ?? throw new InvalidOperationException("Missing GoogleAuth:ClientSecret");
        var callbackPath = googleSection["ExternalCallbackPath"]; // optional, fallback below

        services
            .AddAuthentication() // keep existing defaults from JWT config
            .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
            {
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                // assign string directly; no need for Microsoft.AspNetCore.Http import
                options.CallbackPath = string.IsNullOrWhiteSpace(callbackPath)
                    ? "/signin-google"
                    : callbackPath;
                options.SaveTokens = true;
            });

        return services;
    }

    public static IServiceCollection ConfigureFacebook(this IServiceCollection services, IConfiguration configuration)
    {
        // Tương tự Google: optional external login, không đụng tới JWT default scheme
        var fbSection = configuration.GetSection("FacebookAuth");
        var appId = fbSection["AppId"] ?? throw new InvalidOperationException("Missing FacebookAuth:AppId");
        var appSecret = fbSection["AppSecret"] ?? throw new InvalidOperationException("Missing FacebookAuth:AppSecret");
        var callbackPath = fbSection["ExternalCallbackPath"]; // optional

        services
            .AddAuthentication() // giữ nguyên defaults đã set bởi JWT
            .AddFacebook(FacebookDefaults.AuthenticationScheme, options =>
            {
                options.AppId = appId;
                options.AppSecret = appSecret;
                options.CallbackPath = string.IsNullOrWhiteSpace(callbackPath)
                    ? "/signin-facebook"
                    : callbackPath;

                options.SaveTokens = true;

                // đảm bảo lấy được email/name
                options.Fields.Add("email");
                options.Fields.Add("name");
                options.Scope.Add("email");
            });

        return services;
    }

    public static IServiceCollection ConfigureApikey(this IServiceCollection services, IConfiguration configuration)
    {
        throw new NotImplementedException();
    }

    public static IServiceCollection ConfigureCookie(this IServiceCollection services, IConfiguration configuration)
    {
        throw new NotImplementedException();
    }
}