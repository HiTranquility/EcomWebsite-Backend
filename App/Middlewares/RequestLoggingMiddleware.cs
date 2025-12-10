using System.Diagnostics;

namespace App.Middlewares;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var traceId = Activity.Current?.TraceId.ToString() ?? context.Items["TraceId"]?.ToString() ?? "N/A";
        var requestId = context.Items["RequestId"]?.ToString() ?? "N/A";
        var clientIp = context.Items["ClientIp"]?.ToString() ?? context.Connection.RemoteIpAddress?.ToString() ?? "N/A";

        // Log request
        _logger.LogInformation(
            "Request started: {Method} {Path} QueryString={QueryString} TraceId={TraceId} RequestId={RequestId} ClientIp={ClientIp}",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            traceId,
            requestId,
            clientIp
        );

        try
        {
            await _next(context);
            stopwatch.Stop();

            // Log successful response
            _logger.LogInformation(
                "Request completed: {Method} {Path} StatusCode={StatusCode} Duration={Duration}ms TraceId={TraceId} RequestId={RequestId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                traceId,
                requestId
            );
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            // Log failed request
            _logger.LogError(
                ex,
                "Request failed: {Method} {Path} StatusCode={StatusCode} Duration={Duration}ms TraceId={TraceId} RequestId={RequestId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                traceId,
                requestId
            );
            
            throw;
        }
    }
}

