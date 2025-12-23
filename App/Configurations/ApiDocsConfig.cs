using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using Scalar.AspNetCore;

namespace App.Configurations;

/// <summary>
/// Consolidated API Documentation configuration.
/// Includes: Swagger/OpenAPI + Scalar.
/// </summary>
public static class ApiDocsConfig
{
    /// <summary>
    /// Configure all API documentation services (Swagger + Scalar).
    /// </summary>
    public static IServiceCollection ConfigureApiDocs(this IServiceCollection services)
    {
        // API Versioning
        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V"; // v1, v2
                options.SubstituteApiVersionInUrl = true;
            });

        // Swagger info
        var contact = new OpenApiContact
        {
            Name = "Phat Nguyen",
            Email = "thebeyondtranquility@gmail.com",
            Url = new Uri("https://github.com/HiTranquility")
        };

        var license = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        };

        // SwaggerGen
        services.AddSwaggerGen(c =>
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }

            c.DescribeAllParametersInCamelCase();
            c.EnableAnnotations();

            // JWT Bearer Authentication
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = @"### 🔐 JWT Authorization

Sử dụng Bearer token để xác thực API requests.

**Cách sử dụng:**
1. Đăng nhập qua `/api/v1/auth/login` để lấy token
2. Copy token và paste vào ô bên dưới
3. Token sẽ tự động thêm prefix `Bearer `

**Format:** `Bearer <your_jwt_token>`",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Sort endpoints
            c.OrderActionsBy(desc =>
            {
                var methodOrder = new Dictionary<string, int>
                {
                    { "GET", 1 }, { "POST", 2 }, { "PUT", 3 }, { "PATCH", 4 }, { "DELETE", 5 }
                };
                var order = methodOrder.TryGetValue(desc.HttpMethod?.ToUpperInvariant() ?? "GET", out var o) ? o : 99;
                return $"{desc.ActionDescriptor.RouteValues["controller"]}_{order:D2}_{desc.RelativePath}";
            });

            c.DocInclusionPredicate((docName, apiDesc) =>
                string.Equals(apiDesc.GroupName, docName, StringComparison.OrdinalIgnoreCase));

            c.UseAllOfToExtendReferenceSchemas();
        });

        // Create SwaggerDoc for each API version
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>>(sp =>
        {
            var provider = sp.GetRequiredService<IApiVersionDescriptionProvider>();
            return new ConfigureNamedOptions<SwaggerGenOptions>(string.Empty, options =>
            {
                foreach (var desc in provider.ApiVersionDescriptions)
                {
                    var apiVersion = desc.ApiVersion.ToString();
                    var description = $@"## 📖 EcomWebsite API Documentation

### Version {apiVersion}

This API provides comprehensive endpoints for managing an e-commerce platform including:

- 🔐 **Authentication & Authorization**: User registration, login, JWT tokens
- 👤 **User Management**: User profiles, addresses
- 🛍️ **Product Management**: Products, categories, reviews
- 📝 **Blog Management**: Blog posts, comments
- 🛒 **Order Management**: Shopping cart, checkout
- 💳 **Payment Integration**: Multiple payment gateways

### Base URL
All endpoints are prefixed with `/api/{apiVersion}`";

                    var info = new OpenApiInfo
                    {
                        Title = "EcomWebsite API",
                        Version = apiVersion,
                        Description = desc.IsDeprecated
                            ? description + "\n\n> ⚠️ **DEPRECATED** - This API version is deprecated."
                            : description,
                        Contact = contact,
                        License = license
                    };

                    options.SwaggerDoc(desc.GroupName, info);
                }
            });
        });

        services.AddEndpointsApiExplorer();

        return services;
    }

    /// <summary>
    /// Use Swagger and Scalar API documentation in development/staging.
    /// </summary>
    public static WebApplication UseApiDocs(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment() && !app.Environment.IsStaging())
        {
            return app;
        }

        // Swagger
        app.UseSwagger(c =>
        {
            c.RouteTemplate = "swagger/{documentName}/swagger.json";
        });

        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "EcomWebsite API v1");
            c.SwaggerEndpoint("/swagger/v2/swagger.json", "EcomWebsite API v2");
            c.RoutePrefix = "swagger";
            c.DocumentTitle = "EcomWebsite API - Swagger UI";
            c.DefaultModelsExpandDepth(2);
            c.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Model);
            c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            c.EnableDeepLinking();
            c.EnableFilter();
            c.ShowExtensions();
            c.EnableValidator();
            c.DisplayRequestDuration();
            c.EnableTryItOutByDefault();
            c.ConfigObject.AdditionalItems["syntaxHighlight"] = new Dictionary<string, object>
            {
                { "activated", true },
                { "theme", "monokai" }
            };
        });

        // Scalar - Modern alternative
        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("EcomWebsite API")
                .WithTheme(ScalarTheme.BluePlanet)
                .WithDarkMode(true)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                .WithModels(false);
        });

        return app;
    }
}
