using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpLogging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace App.Configurations;

/// <summary>
/// Consolidated Logging configuration.
/// Includes: Serilog, OpenTelemetry (Logs/Metrics/Tracing), HTTP Logging.
/// </summary>
public static class LoggingConfig
{
    /// <summary>
    /// Configure all logging services (Serilog, OpenTelemetry, HTTP Logging).
    /// </summary>
    public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;
        var environment = builder.Environment;
        
        // 1. Serilog - Primary logging
        ConfigureSerilogInternal(builder, configuration, environment);
        
        // 2. OpenTelemetry - Observability (optional)
        ConfigureOpenTelemetryInternal(builder.Services, configuration);
        
        // 3. HTTP Logging - Request/Response logging (optional)
        ConfigureHttpLoggingInternal(builder.Services, configuration);
        
        return builder;
    }

    #region --- Serilog ---
    
    private static void ConfigureSerilogInternal(WebApplicationBuilder builder, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var seqUrl = Environment.GetEnvironmentVariable("Seq__ServerUrl")
            ?? configuration["Seq:ServerUrl"];
        var seqApiKey = Environment.GetEnvironmentVariable("Seq__ApiKey")
            ?? configuration["Seq:ApiKey"];
        
        var minLevel = GetLogLevel(configuration);
        
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(minLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithProcessId()
            .Enrich.WithProperty("Application", "EcomWebsite-Backend")
            .Enrich.WithProperty("Environment", environment.EnvironmentName);
        
        // Console sink - human readable format for all environments
        // JSON format is used for file logs which can be parsed by log aggregators
        loggerConfig.WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}      {Message:lj}{NewLine}{Exception}");
        
        // File sink - rolling daily
        var logPath = configuration["Logging:FilePath"] ?? "logs/ecomwebsite-.log";
        loggerConfig.WriteTo.File(
            new CompactJsonFormatter(),
            logPath,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            fileSizeLimitBytes: 100 * 1024 * 1024,
            rollOnFileSizeLimit: true);
        
        // Seq sink - if configured
        if (!string.IsNullOrWhiteSpace(seqUrl) && !seqUrl.Contains("${"))
        {
            if (!string.IsNullOrWhiteSpace(seqApiKey) && !seqApiKey.Contains("${"))
            {
                loggerConfig.WriteTo.Seq(seqUrl, apiKey: seqApiKey);
            }
            else
            {
                loggerConfig.WriteTo.Seq(seqUrl);
            }
        }
        
        Log.Logger = loggerConfig.CreateLogger();
        builder.Host.UseSerilog();
    }
    
    private static LogEventLevel GetLogLevel(IConfiguration configuration)
    {
        var levelStr = Environment.GetEnvironmentVariable("Logging__MinimumLevel")
            ?? configuration["Logging:MinimumLevel"]
            ?? "Information";
            
        return levelStr.ToLowerInvariant() switch
        {
            "verbose" or "trace" => LogEventLevel.Verbose,
            "debug" => LogEventLevel.Debug,
            "information" or "info" => LogEventLevel.Information,
            "warning" or "warn" => LogEventLevel.Warning,
            "error" => LogEventLevel.Error,
            "fatal" or "critical" => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }
    
    #endregion

    #region --- OpenTelemetry ---
    
    private static void ConfigureOpenTelemetryInternal(IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("OpenTelemetry");
        
        if (!section.GetValue<bool>("Enabled"))
        {
            return;
        }

        var otlpEndpoint = section.GetValue<string>("OtlpEndpoint");
        var serviceName = section.GetValue<string>("ServiceName") ?? "EcomWebsite-Backend";

        void ConfigureOtlpExporter(OtlpExporterOptions options)
        {
            if (!string.IsNullOrWhiteSpace(otlpEndpoint))
            {
                options.Endpoint = new Uri(otlpEndpoint);
            }
            options.Protocol = OtlpExportProtocol.Grpc;
        }

        // Logs
        services.AddLogging(lb =>
        {
            lb.AddOpenTelemetry(o =>
            {
                o.IncludeScopes = true;
                o.ParseStateValues = true;
                o.IncludeFormattedMessage = true;
                o.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName));
                o.AddOtlpExporter(ConfigureOtlpExporter);
            });
        });

        // Metrics & Tracing
        services.AddOpenTelemetry()
            .ConfigureResource(rb => rb.AddService(serviceName))
            .WithMetrics(b =>
            {
                b.AddAspNetCoreInstrumentation();
                b.AddRuntimeInstrumentation();
                b.AddMeter("App.Cache");
                b.AddMeter("Microsoft.EntityFrameworkCore");
                b.AddOtlpExporter(ConfigureOtlpExporter);
            })
            .WithTracing(t =>
            {
                t.AddAspNetCoreInstrumentation();
                t.AddHttpClientInstrumentation();
                t.AddSource("App.Cache");
                t.AddOtlpExporter(ConfigureOtlpExporter);
            });
    }
    
    #endregion

    #region --- HTTP Logging ---
    
    private static void ConfigureHttpLoggingInternal(IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("HttpLogging");
        var enabled = section.GetValue<bool?>("Enabled") ?? false;
        
        if (!enabled) return;

        services.AddHttpLogging(options =>
        {
            var loggingFields = HttpLoggingFields.RequestPropertiesAndHeaders | HttpLoggingFields.ResponsePropertiesAndHeaders;

            if (section.GetValue<bool>("IncludeRequestBody"))
            {
                loggingFields |= HttpLoggingFields.RequestBody;
            }

            if (section.GetValue<bool>("IncludeResponseBody"))
            {
                loggingFields |= HttpLoggingFields.ResponseBody;
            }

            options.LoggingFields = loggingFields;
            options.RequestBodyLogLimit = section.GetValue<int?>("RequestBodyLogLimit") ?? 4096;
            options.ResponseBodyLogLimit = section.GetValue<int?>("ResponseBodyLogLimit") ?? 4096;
            options.CombineLogs = false;

            options.MediaTypeOptions.AddText("application/json");
            options.MediaTypeOptions.AddText("text/plain");
        });
    }
    
    #endregion

    #region --- Middleware Extensions ---
    
    /// <summary>
    /// Add Serilog request logging middleware with custom configuration.
    /// </summary>
    public static WebApplication UseSerilogRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            
            options.GetLevel = (httpContext, elapsed, ex) =>
            {
                if (ex != null) return LogEventLevel.Error;
                if (httpContext.Response.StatusCode >= 500) return LogEventLevel.Error;
                if (httpContext.Response.StatusCode >= 400) return LogEventLevel.Warning;
                return LogEventLevel.Information;
            };
            
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? "unknown");
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString() ?? "unknown");
                diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
                
                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    diagnosticContext.Set("UserId", httpContext.User.Identity.Name ?? "unknown");
                }
            };
        });
        
        return app;
    }
    
    #endregion
}
