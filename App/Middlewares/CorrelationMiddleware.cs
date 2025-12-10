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
        // 1) TraceId - ưu tiên dùng từ OpenTelemetry Activity, nếu không có thì lấy từ header hoặc tạo mới
        var traceId = Activity.Current?.TraceId.ToString() 
            ?? context.Request.Headers["X-Trace-Id"].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        // 2) RequestId - tạo mới cho mỗi request tại service này
        var requestId = Guid.NewGuid().ToString();

        // 3) Đưa vào response headers để propagate cho service khác
        context.Response.Headers["X-Trace-Id"] = traceId;
        context.Response.Headers["X-Request-Id"] = requestId;

        // 4) Gắn vào HttpContext.Items để code bên dưới có thể lấy
        context.Items["TraceId"] = traceId;
        context.Items["RequestId"] = requestId;

        // 5) Thêm vào Activity tags nếu có OpenTelemetry
        if (Activity.Current != null)
        {
            Activity.Current.SetTag("request.id", requestId);
            Activity.Current.SetTag("trace.id", traceId);
        }

        await _next(context);
    }
}