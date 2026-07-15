// ============================================================================
// Copyright (c) 2026 Nguyen Tan Phat (HiTranquility). All rights reserved.
// This source code is proprietary and confidential.
// Unauthorized copying, modification, or distribution is strictly prohibited.
// Contact: HiTranquility
// ============================================================================
using App.Middlewares;
using App.Configurations;
using App.INFRA.BackgroundJobs;
using Serilog;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System.Text.Json;

// ============================================
// Application Builder
// ============================================

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// ============================================
// 🔧 SERVICE CONFIGURATION (Consolidated)
// ============================================

#region --- Logging (Serilog + OpenTelemetry) ---
builder.ConfigureLogging();
#endregion

#region --- Controllers + Cache Profiles ---
builder.Services.AddControllers()
    .ConfigureCacheProfiles();
builder.Services.AddOpenApi();
#endregion

#region --- Dependency Injection (Repos, Services, Mappers) ---
builder.Services.ConfigureService(configuration);
#endregion

#region --- API Documentation (Swagger + Scalar) ---
builder.Services.ConfigureApiDocs();
#endregion

#region --- Database (EF Core + Seeding) ---
builder.Services.ConfigurePersistence(configuration);
#endregion

#region --- Security (Auth + Authorization) ---
builder.Services.ConfigureJwt(configuration);
builder.Services.ConfigureGoogle(configuration);
// builder.Services.ConfigureFacebook(configuration);
#endregion

#region --- Web (CORS, RateLimiting, Compression, Validation, Caching) ---
builder.Services.ConfigureWeb(configuration);
#endregion

#region --- Infrastructure (Cache, Queue, Payment, Jobs, Health, Resilience) ---
builder.Services.ConfigureHybridCache(configuration);
builder.Services.ConfigureMessaging(configuration);  // Handles RabbitMQ/Kafka with conditional DI
builder.Services.ConfigurePayment(configuration);
builder.Services.ConfigureHealthChecks(configuration);
builder.Services.ConfigureResilience(configuration);
builder.Services.ConfigureHangfire(configuration);
builder.Services.AddScoped<IBackgroundJobService, BackgroundJobService>();
#endregion

#region --- Cross-Cutting (MediatR, AuditLogging) ---
builder.Services.ConfigureMediatR();
builder.Services.ConfigureAuditLogging();
#endregion


// ============================================
// Application Pipeline
// ============================================

// ============================================
// AWS Secrets Manager Integration
// ============================================
if (builder.Environment.IsDevelopment())
{
    var client = new AmazonSecretsManagerClient(RegionEndpoint.USEast1);
    var resp = client.GetSecretValueAsync(
        new GetSecretValueRequest { SecretId = "eshop/dev" }).GetAwaiter().GetResult();
    var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(resp.SecretString);
    builder.Configuration.AddInMemoryCollection(dict!);
    Console.WriteLine("Fetched secret eshop/dev from AWS Secrets Manager successfully.");
}

var app = builder.Build();

// ============================================
// 🔌 MIDDLEWARE PIPELINE (Order Matters!)
// ============================================

// 1. API Documentation (Dev/Staging only)
app.UseApiDocs();

// 2. HSTS (Production only)
if (app.Environment.IsProduction())
{
    app.UseHsts();
}

// 3. Serilog Request Logging
app.UseSerilogRequestLogging();

// 4. Exception Handler
app.UseMiddleware<ExceptionMiddleware>();

// 5. Response Compression
app.UseResponseCompression();

// 6. Rate Limiting
app.UseRateLimiter();

// 7. Routing
app.UseRouting();

// 8. CORS
app.UseCors(app.Environment.IsDevelopment() ? "CorsDevPolicy" : "CorsRestrictedPolicy");

// 9. Session (if enabled)
// app.UseSession();

// 10. Authentication
app.UseAuthentication();

// 11. Authorization
app.UseAuthorization();

// 12. Response Caching
app.UseResponseCachingMiddleware();

// ============================================
// 📍 ENDPOINTS
// ============================================

// Health Checks
app.MapHealthCheckEndpoints();

// API Controllers
app.MapControllers();

// Hangfire Dashboard (Dev/Staging only)
app.UseHangfireDashboard(configuration);

// ============================================
// 🚀 RUN APPLICATION
// ============================================

try
{
    Log.Information("Starting EcomWebsite API...");
    Console.WriteLine("[HiTranquility] EcomWebsite API initialized.");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

