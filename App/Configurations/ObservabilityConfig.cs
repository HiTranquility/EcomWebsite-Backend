using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using Microsoft.AspNetCore.HttpLogging; 
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace App.Configurations;

public static class ObservabilityConfig
{
    // --- HTTP Logging ---
    public static IServiceCollection ConfigureHttpLogging(this IServiceCollection services, IConfiguration configuration)
    {
        var httpLogSection = configuration.GetSection("HttpLogging");
        var httpLoggingEnabled = httpLogSection.GetValue<bool?>("Enabled") ?? false;
        if (!httpLoggingEnabled) return services;

        var includeReqBody = httpLogSection.GetValue<bool?>("IncludeRequestBody") ?? false;
        var includeResBody = httpLogSection.GetValue<bool?>("IncludeResponseBody") ?? false;
        var reqBodyLimit = httpLogSection.GetValue<int?>("RequestBodyLogLimit") ?? 4096;
        var resBodyLimit = httpLogSection.GetValue<int?>("ResponseBodyLogLimit") ?? 4096;

        services.AddHttpLogging(options =>
        {
            var fields = HttpLoggingFields.RequestPropertiesAndHeaders | HttpLoggingFields.ResponsePropertiesAndHeaders;
            if (includeReqBody) fields |= HttpLoggingFields.RequestBody;
            if (includeResBody) fields |= HttpLoggingFields.ResponseBody;
            options.LoggingFields = fields;

            options.RequestBodyLogLimit = reqBodyLimit;
            options.ResponseBodyLogLimit = resBodyLimit;
            options.CombineLogs = false;

            options.MediaTypeOptions.AddText("application/json");
            options.MediaTypeOptions.AddText("text/plain");
        });

        return services;
    }

    // --- OpenTelemetry (Logs / Metrics / Tracing) ---
    public static IServiceCollection ConfigureOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var otelSection = configuration.GetSection("OpenTelemetry");
        var enabled = otelSection.GetValue<bool?>("Enabled") ?? false;
        if (!enabled) return services;

        var otlpEndpoint = otelSection["OtlpEndpoint"];
        var serviceName = otelSection["ServiceName"];

        // Logs
        services.AddLogging(lb =>
        {
            lb.AddOpenTelemetry(o =>
            {
                o.IncludeScopes = true;
                o.ParseStateValues = true;
                o.IncludeFormattedMessage = true;

                if (!string.IsNullOrWhiteSpace(serviceName))
                    o.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: serviceName));

                o.AddOtlpExporter(exp =>
                {
                    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                        exp.Endpoint = new Uri(otlpEndpoint);
                    exp.Protocol = OtlpExportProtocol.Grpc;
                });
            });
        });

        // Metrics & Tracing
        services.AddOpenTelemetry()
            .ConfigureResource(rb =>
            {
                if (!string.IsNullOrWhiteSpace(serviceName))
                    rb.AddService(serviceName: serviceName);
            })
            .WithMetrics(b =>
            {
                b.AddAspNetCoreInstrumentation();
                b.AddRuntimeInstrumentation();
                b.AddMeter("App.Cache");
                b.AddMeter("Microsoft.EntityFrameworkCore");

                b.AddOtlpExporter(o =>
                {
                    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                        o.Endpoint = new Uri(otlpEndpoint);
                    o.Protocol = OtlpExportProtocol.Grpc;
                });
            })
            .WithTracing(t =>
            {
                t.AddAspNetCoreInstrumentation();
                t.AddHttpClientInstrumentation();
                t.AddSource("App.Cache"); 

                t.AddOtlpExporter(o =>
                {
                    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                        o.Endpoint = new Uri(otlpEndpoint);
                    o.Protocol = OtlpExportProtocol.Grpc;
                });
            });

        return services;
    }
}