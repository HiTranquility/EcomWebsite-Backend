namespace App.Middlewares;

public class ClientInfoMiddleware
{
    private readonly RequestDelegate _next;

    public ClientInfoMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract client IP address
        // Priority: X-Forwarded-For > X-Real-IP > RemoteIpAddress
        var clientIp = GetClientIpAddress(context);
        // Store in HttpContext.Items for easy access throughout the request pipeline
        context.Items["ClientIp"] = clientIp;
        await _next(context);
    }

    private string? GetClientIpAddress(HttpContext context)
    {
        // Check X-Forwarded-For header (for requests behind proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            // X-Forwarded-For can contain multiple IPs, take the first one
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (ips.Length > 0 && !string.IsNullOrWhiteSpace(ips[0]))
            {
                return ips[0];
            }
        }

        // Check X-Real-IP header
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(realIp))
        {
            return realIp;
        }

        // Fallback to RemoteIpAddress
        return context.Connection.RemoteIpAddress?.ToString();
    }
}

