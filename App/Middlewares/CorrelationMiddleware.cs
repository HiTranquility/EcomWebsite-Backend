using System.Diagnostics;

namespace App.Middlewares;

public class CorrelationMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1) TraceId (xuyên suốt nhiều service) -> dùng Activity.Current hoặc sinh mới
        var traceId = Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString();

        // 2) RequestId (cho từng request tại service này)
        var requestId = Guid.NewGuid().ToString();

        // 3) Đưa vào header để propagate cho service khác
        context.Response.Headers["X-Trace-Id"] = traceId;
        context.Response.Headers["X-Request-Id"] = requestId;

        // 4) Gắn vào HttpContext để code bên dưới lấy
        context.Items["TraceId"] = traceId;
        context.Items["RequestId"] = requestId;

        await _next(context);
    }
}