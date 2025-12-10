using System.Diagnostics;

namespace App.Middlewares;

public class RequestValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestValidationMiddleware> _logger;

    // Configuration constants
    private const long MaxRequestSize = 10 * 1024 * 1024; // 10 MB
    private const long MaxFormSize = 5 * 1024 * 1024; // 5 MB
    private const int MaxQueryStringLength = 2048;

    public RequestValidationMiddleware(RequestDelegate next, ILogger<RequestValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        var traceId = Activity.Current?.TraceId.ToString() ?? context.Items["TraceId"]?.ToString() ?? "N/A";
        var requestId = context.Items["RequestId"]?.ToString() ?? "N/A";

        // Validate Content-Length
        if (request.ContentLength.HasValue && request.ContentLength.Value > MaxRequestSize)
        {
            _logger.LogWarning(
                "Request rejected: Content-Length too large. Size={Size} Max={Max} TraceId={TraceId} RequestId={RequestId}",
                request.ContentLength.Value,
                MaxRequestSize,
                traceId,
                requestId
            );

            context.Response.StatusCode = 413; // Payload Too Large
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = $"Request body size exceeds maximum allowed size of {MaxRequestSize / (1024 * 1024)} MB",
                status = 413
            });
            return;
        }

        // Validate Query String length
        if (request.QueryString.HasValue && request.QueryString.Value?.Length > MaxQueryStringLength)
        {
            _logger.LogWarning(
                "Request rejected: Query string too long. Length={Length} Max={Max} TraceId={TraceId} RequestId={RequestId}",
                request.QueryString.Value.Length,
                MaxQueryStringLength,
                traceId,
                requestId
            );

            context.Response.StatusCode = 414; // URI Too Long
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = $"Query string length exceeds maximum allowed length of {MaxQueryStringLength} characters",
                status = 414
            });
            return;
        }

        // Validate Form size (if it's a form request)
        if (request.HasFormContentType && request.ContentLength.HasValue && request.ContentLength.Value > MaxFormSize)
        {
            _logger.LogWarning(
                "Request rejected: Form size too large. Size={Size} Max={Max} TraceId={TraceId} RequestId={RequestId}",
                request.ContentLength.Value,
                MaxFormSize,
                traceId,
                requestId
            );

            context.Response.StatusCode = 413; // Payload Too Large
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = $"Form data size exceeds maximum allowed size of {MaxFormSize / (1024 * 1024)} MB",
                status = 413
            });
            return;
        }

        // Validate required headers for API requests (optional, customize as needed)
        if (request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            // Example: Require User-Agent for API requests
            if (string.IsNullOrWhiteSpace(request.Headers["User-Agent"].ToString()))
            {
                _logger.LogWarning(
                    "Request rejected: Missing User-Agent header. TraceId={TraceId} RequestId={RequestId}",
                    traceId,
                    requestId
                );

                context.Response.StatusCode = 400; // Bad Request
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "User-Agent header is required for API requests",
                    status = 400
                });
                return;
            }
        }

        await _next(context);
    }
}

