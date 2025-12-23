using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using App.UTIL.Abstractions.DTO.Response;

namespace App.Configurations;

/// <summary>
/// Combined Authentication and Authorization configuration.
/// Handles JWT, OAuth providers (Google, Facebook), and Authorization policies.
/// </summary>
public static class AuthConfig
{
    #region --- Role & Permission Constants ---
    
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Editor = "Editor";
        public const string Support = "Support";
        public const string Customer = "Customer";

        public static readonly string[] All = { Admin, Manager, Editor, Support, Customer };
    }

    public static class Permissions
    {
        public const string UsersRead = "users.read";
        public const string UsersManage = "users.manage";
        public const string BlogsRead = "blogs.read";
        public const string BlogsPublish = "blogs.publish";
        public const string ProductsRead = "products.read";
        public const string ProductsManage = "products.manage";
        public const string OrdersRead = "orders.read";
        public const string OrdersManage = "orders.manage";
        public const string OrdersCreate = "orders.create";
        public const string OrdersCancel = "orders.cancel";
        public const string OrdersFulfill = "orders.fulfill";
        public const string ReportsView = "reports.view";
        public const string ReportsExport = "reports.export";
        public const string SettingsManage = "settings.manage";

        public static readonly string[] All =
        {
            UsersRead,
            UsersManage,
            BlogsRead,
            BlogsPublish,
            ProductsRead,
            ProductsManage,
            OrdersRead,
            OrdersManage,
            OrdersCreate,
            OrdersCancel,
            OrdersFulfill,
            ReportsView,
            ReportsExport,
            SettingsManage
        };
    }

    public static class Policies
    {
        public const string AdminOnly = "AdminOnly";
        public const string ManagerOrAdmin = "ManagerOrAdmin";
        public const string SupportOnly = "SupportOnly";

        public const string PermissionPrefix = "Permission:";

        public static string Permission(string permissionKey) => $"{PermissionPrefix}{permissionKey}";

        public static readonly string UsersManage = Permission(Permissions.UsersManage);
        public static readonly string BlogsPublish = Permission(Permissions.BlogsPublish);
        public static readonly string OrdersManage = Permission(Permissions.OrdersManage);
        public static readonly string OrdersFulfill = Permission(Permissions.OrdersFulfill);
        public static readonly string ReportsView = Permission(Permissions.ReportsView);
        public static readonly string ReportsExport = Permission(Permissions.ReportsExport);
        public static readonly string SettingsManage = Permission(Permissions.SettingsManage);
    }
    
    #endregion

    #region --- JWT Configuration ---
    
    public static IServiceCollection ConfigureJwt(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("JwtSettings");
        
        var issuer = section.GetValue<string>("Issuer")
            ?? throw new InvalidOperationException("Missing JwtSettings:Issuer");
        var audience = section.GetValue<string>("Audience")
            ?? throw new InvalidOperationException("Missing JwtSettings:Audience");
        var secret = section.GetValue<string>("SecretKey")
            ?? throw new InvalidOperationException("Missing JwtSettings:SecretKey");

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
    
    #endregion

    #region --- OAuth Providers Configuration ---
    
    public static IServiceCollection ConfigureGoogle(this IServiceCollection services, IConfiguration configuration)
    {
        // Note: This config is optional. Our app primarily uses JWT + manual Google code/id_token exchange.
        // If you enable this, do NOT override default schemes already set by JWT.
        var section = configuration.GetSection("GoogleAuth");
        
        var clientId = section.GetValue<string>("ClientId")
            ?? throw new InvalidOperationException("Missing GoogleAuth:ClientId");
        var clientSecret = section.GetValue<string>("ClientSecret")
            ?? throw new InvalidOperationException("Missing GoogleAuth:ClientSecret");
        
        // CallbackPath needs to be a path (starting with '/'), not a full URL
        var callbackPath = section.GetValue<string>("ExternalCallbackPath") ?? "/signin-google";
        if (!callbackPath.StartsWith("/", StringComparison.Ordinal))
        {
            callbackPath = "/" + callbackPath;
        }

        services
            .AddAuthentication() // keep existing defaults from JWT config
            .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
            {
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                options.CallbackPath = callbackPath;
                options.SaveTokens = true;
            });

        return services;
    }

    /*
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
    */

    public static IServiceCollection ConfigureApikey(this IServiceCollection services, IConfiguration configuration)
    {
        throw new NotImplementedException();
    }

    public static IServiceCollection ConfigureCookie(this IServiceCollection services, IConfiguration configuration)
    {
        throw new NotImplementedException();
    }
    
    #endregion

    #region --- Authorization Policies Configuration ---
    
    /// <summary>
    /// Đăng ký các policy dựa trên Role và Permission.
    /// </summary>
    public static IServiceCollection ConfigureAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.AdminOnly, policy => policy.RequireRole(Roles.Admin));
            options.AddPolicy(Policies.ManagerOrAdmin, policy => policy.RequireRole(Roles.Manager, Roles.Admin));
            options.AddPolicy(Policies.SupportOnly, policy => policy.RequireRole(Roles.Support));

            foreach (var permission in Permissions.All)
            {
                options.AddPolicy(Policies.Permission(permission),
                    policy => policy.RequireClaim("Permission", permission));
            }
        });

        return services;
    }
    
    #endregion
}
