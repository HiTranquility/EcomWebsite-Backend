namespace App.Middlewares;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var response = context.Response;

        // Prevent MIME type sniffing
        response.Headers["X-Content-Type-Options"] = "nosniff";

        // Prevent clickjacking
        response.Headers["X-Frame-Options"] = "DENY";

        // Enable XSS protection (legacy but still useful)
        response.Headers["X-XSS-Protection"] = "1; mode=block";

        // Referrer Policy
        response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Permissions Policy (formerly Feature-Policy)
        response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";

        // Remove server header for security
        response.Headers.Remove("Server");
        response.Headers.Remove("X-Powered-By");

        // HSTS - only add in production with HTTPS
        if (context.Request.IsHttps && !context.Request.Host.Host.Contains("localhost"))
        {
            response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
        }

        // Content Security Policy - adjust based on your needs
        // This is a basic CSP, you may need to customize it
        if (!context.Response.Headers.ContainsKey("Content-Security-Policy"))
        {
            response.Headers["Content-Security-Policy"] = 
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data: https:; " +
                "font-src 'self' data:; " +
                "connect-src 'self'; " +
                "frame-ancestors 'none';";
        }

        await _next(context);
    }
}

