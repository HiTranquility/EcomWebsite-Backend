using Microsoft.AspNetCore.Http;

namespace App.UTIL.Extensions;

public static class HttpContextExtensions
{
    /// <summary>
    /// Gets the client IP address from ClientInfoMiddleware (stored in HttpContext.Items["ClientIp"])
    /// </summary>
    public static string? GetClientIp(this HttpContext context)
    {
        return context.Items["ClientIp"]?.ToString();
    }

    /// <summary>
    /// Gets the refresh token from headers or cookies
    /// </summary>
    public static string? GetRefreshToken(this HttpContext context)
    {
        return context.Request.Headers["Refresh-Token"].FirstOrDefault()
            ?? context.Request.Cookies["refreshToken"];
    }

    /// <summary>
    /// Gets the User-Agent string from UserAgentMiddleware
    /// </summary>
    public static string? GetUserAgent(this HttpContext context)
    {
        return context.Items["UserAgent"]?.ToString();
    }

    /// <summary>
    /// Gets the browser name from UserAgentMiddleware
    /// </summary>
    public static string? GetBrowser(this HttpContext context)
    {
        return context.Items["Browser"]?.ToString();
    }

    /// <summary>
    /// Gets the browser version from UserAgentMiddleware
    /// </summary>
    public static string? GetBrowserVersion(this HttpContext context)
    {
        return context.Items["BrowserVersion"]?.ToString();
    }

    /// <summary>
    /// Gets the operating system from UserAgentMiddleware
    /// </summary>
    public static string? GetOperatingSystem(this HttpContext context)
    {
        return context.Items["OperatingSystem"]?.ToString();
    }

    /// <summary>
    /// Gets the device type (Desktop, Mobile, Tablet) from UserAgentMiddleware
    /// </summary>
    public static string? GetDeviceType(this HttpContext context)
    {
        return context.Items["DeviceType"]?.ToString();
    }

    /// <summary>
    /// Checks if the request is from a mobile device
    /// </summary>
    public static bool IsMobile(this HttpContext context)
    {
        return context.Items["IsMobile"] is bool isMobile && isMobile;
    }

    /// <summary>
    /// Checks if the request is from a bot/crawler
    /// </summary>
    public static bool IsBot(this HttpContext context)
    {
        return context.Items["IsBot"] is bool isBot && isBot;
    }
}

