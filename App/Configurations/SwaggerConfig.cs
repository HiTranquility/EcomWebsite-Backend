using Microsoft.OpenApi.Models;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace App.Configurations;

public static class SwaggerConfig
{
    public static IServiceCollection ConfigureSwagger(this IServiceCollection services)
    {
        // --- 1. API Versioning ---
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

        // --- 2. Thông tin Swagger ---
        var contact = new OpenApiContact
        {
            Name = "Phat Nguyen",
            Email = "thebeyondtranquility@gmail.com",
            Url = new Uri("https://github.com/your-username")
        };

        var license = new OpenApiLicense
        {
            Name = "Apache 2.0",
            Url = new Uri("http://www.apache.org/licenses/LICENSE-2.0.html")
        };

        var terms = new Uri("https://example.com/terms");

        // --- 3. SwaggerGen với cải thiện ---
        services.AddSwaggerGen(c =>
        {
            // XML Comments để hiển thị documentation (nếu có)
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            // Describe all parameters in camelCase
            c.DescribeAllParametersInCamelCase();

            // JWT Bearer Authentication (cải thiện description)
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = @"JWT Authorization header using the Bearer scheme. 
                              Enter 'Bearer' [space] and then your token in the text input below.
                              Example: 'Bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
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
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });

            // Sort endpoints by HTTP method (cải thiện)
            c.OrderActionsBy(desc => 
            {
                var methodOrder = new Dictionary<string, int>
                {
                    { "GET", 1 },
                    { "POST", 2 },
                    { "PUT", 3 },
                    { "PATCH", 4 },
                    { "DELETE", 5 }
                };
                return methodOrder.TryGetValue(desc.HttpMethod?.ToString() ?? "GET", out var order) ? order.ToString() : "99";
            });

            // Ensure each API description only appears in its own versioned doc
            c.DocInclusionPredicate((docName, apiDesc) => string.Equals(apiDesc.GroupName, docName, StringComparison.OrdinalIgnoreCase));
        });

        // Create SwaggerDoc for each API version (cải thiện với description đầy đủ)
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>>(sp =>
        {
            var provider = sp.GetRequiredService<IApiVersionDescriptionProvider>();
            return new ConfigureNamedOptions<SwaggerGenOptions>(string.Empty, options =>
            {
                foreach (var desc in provider.ApiVersionDescriptions)
                {
                    var info = new OpenApiInfo
                    {
                        Title = "EcomWebsite API",
                        Version = desc.ApiVersion.ToString(),
                        Description = @"
## EcomWebsite RESTful API

This API provides endpoints for managing an e-commerce website including:
- **User Management**: Authentication, authorization, and user profiles
- **Product Management**: Products, categories, reviews, and inventory
- **Order Management**: Shopping cart, checkout, and order tracking
- **Blog Management**: Blog posts, comments, and categories

### Authentication
Most endpoints require JWT authentication. Use the **Authorize** button above to authenticate.

### API Versions
- **v1**: Current stable version
- **v2**: Latest version with new features
                        ",
                        Contact = contact,
                        License = license,
                        TermsOfService = terms
                    };

                    if (desc.IsDeprecated)
                    {
                        info.Description += "\n\n⚠️ **This API version is deprecated and will be removed in a future release.**";
                    }

                    options.SwaggerDoc(desc.GroupName, info);
                }
            });
        });

        services.AddEndpointsApiExplorer();

        return services;
    }
}