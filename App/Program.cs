using App.Middlewares;
using App.Configurations;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
#region --- Dependency Injection ---

builder.Services.ConfigureService(configuration);

#endregion

#region --- Swagger Configuration ---

builder.Services.ConfigureSwagger();

#endregion

#region --- Observability Configuration ---

// OpenTelemetry Configuration
builder.Services.ConfigureOpenTelemetry(configuration);
// Http Logging Configuration
builder.Services.ConfigureHttpLogging(configuration);

#endregion

#region --- Database Configuration ---

builder.Services.ConfigurePersistence(configuration);
// Database initialization is handled after build (fluent extension)

#endregion

#region --- Caching Configuration ---
builder.Services.ConfigureHybridCache(configuration);
#endregion

#region --- Session Configuration ---

builder.Services.ConfigureSession(configuration);

#endregion

#region --- Security Configuration ---
builder.Services.ConfigureJwt(configuration);
builder.Services.ConfigureGoogle(configuration);
/*
builder.Services.ConfigureFacebook(configuration);
*/
builder.Services.ConfigureAuthorizationPolicies();
#endregion

#region --- Validation Configuration ---
builder.Services.ConfigureFluentValidation();
#endregion

#region --- Other Configuration ---
builder.Services.ConfigureApiBehavior();
builder.Services.ConfigureCors(configuration);
builder.Services.ConfigureRateLimiting(configuration);
builder.Services.ConfigureResponseCompression();
#endregion



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "My API v2");
    });
}
else if (app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "My API v2");
    });
}
else if (app.Environment.IsProduction())
{

}
else
{
    app.UseHsts();
}

// ============================================
// Middleware Pipeline - Order Matters!
// ============================================

// // 1. Correlation Middleware - must run early to set TraceId/RequestId for all downstream middleware
// app.UseMiddleware<CorrelationMiddleware>();

// // 2. Request Validation - validate request size, headers early (reject bad requests before processing)
// app.UseMiddleware<RequestValidationMiddleware>();

// // 4. Request Logging - needs TraceId/RequestId from CorrelationMiddleware
// app.UseMiddleware<RequestLoggingMiddleware>();

// // 5. Security Headers - add security headers early in pipeline
// app.UseMiddleware<SecurityHeadersMiddleware>();

// // 6. Client Info - extract client IP (useful for security/analytics)
// app.UseMiddleware<ClientInfoMiddleware>();

// // 7. User Agent - parse user agent info
// app.UseMiddleware<UserAgentMiddleware>();

// 8. Exception Handler - Custom Middleware to catch and handle all exceptions
app.UseMiddleware<ExceptionMiddleware>();    

// // 9. Response Compression - compress responses to reduce bandwidth
// app.UseResponseCompression();

// // 10. Cookie Policy
// app.UseCookiePolicy();

// 11. Routing
app.UseRouting();

// 12. Session
app.UseSession();

// 13. CORS
app.UseCors(app.Environment.IsDevelopment() ? "CorsDevPolicy" : "CorsRestrictedPolicy");

// 14. Authentication
app.UseAuthentication();

// 15. Authorization
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok("OK"));
app.MapControllers();
app.Run();