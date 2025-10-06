using App.Middlewares;
using App.Configurations;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
#region --- Dependency Injection ---

builder.Services.ConfigureService();

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

#region --- Security Configuration ---
builder.Services.ConfigureJwt(configuration);
builder.Services.AddAuthorization();
#endregion

#region --- Other Configuration ---
builder.Services.ConfigureApiBehavior();
builder.Services.ConfigureCors(configuration);
builder.Services.ConfigureRateLimiting(configuration);
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
}
else if (app.Environment.IsProduction())
{

}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

//app.UseMiddleware<ValidationMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();
//app.UseMiddleware<AuthenticationMiddleware>();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(app.Environment.IsDevelopment() ? "CorsDevPolicy" : "CorsRestrictedPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok("OK"));
app.MapControllers();
app.Run();