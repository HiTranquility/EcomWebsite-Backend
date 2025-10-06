using Microsoft.OpenApi.Models;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

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
            Email = "thebeyondtranquility@gmail.com"
        };

        var license = new OpenApiLicense
        {
            Name = "Apache 2.0",
            Url = new Uri("http://www.apache.org/licenses/LICENSE-2.0.html")
        };

        var terms = new Uri("https://example.com/terms");

        // --- 4. SwaggerGen cơ bản ---
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
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

            c.OrderActionsBy(desc => desc.HttpMethod);

            // Ensure each API description only appears in its own versioned doc
            c.DocInclusionPredicate((docName, apiDesc) => string.Equals(apiDesc.GroupName, docName, StringComparison.OrdinalIgnoreCase));
        });

        // Create SwaggerDoc for each API version
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>>(sp =>
        {
            var provider = sp.GetRequiredService<IApiVersionDescriptionProvider>();
            return new ConfigureNamedOptions<SwaggerGenOptions>(string.Empty, options =>
            {
                foreach (var desc in provider.ApiVersionDescriptions)
                {
                    options.SwaggerDoc(desc.GroupName, new OpenApiInfo
                    {
                        Title = "App",
                        Version = desc.GroupName
                    });
                }
            });
        });

        services.AddEndpointsApiExplorer();

        return services;
    }
}
