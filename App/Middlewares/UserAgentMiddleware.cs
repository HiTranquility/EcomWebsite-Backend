using System.Text.RegularExpressions;

namespace App.Middlewares;

public class UserAgentMiddleware
{
    private readonly RequestDelegate _next;

    public UserAgentMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract User-Agent header
        var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault();
        
        // Parse User-Agent to extract browser, OS, and device info
        var clientInfo = ParseUserAgent(userAgent);
        
        // Store in HttpContext.Items for easy access throughout the request pipeline
        context.Items["UserAgent"] = userAgent;
        context.Items["Browser"] = clientInfo.Browser;
        context.Items["BrowserVersion"] = clientInfo.BrowserVersion;
        context.Items["OperatingSystem"] = clientInfo.OperatingSystem;
        context.Items["DeviceType"] = clientInfo.DeviceType;
        context.Items["IsMobile"] = clientInfo.IsMobile;
        context.Items["IsBot"] = clientInfo.IsBot;

        await _next(context);
    }

    private ClientInfo ParseUserAgent(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return new ClientInfo();
        }

        var ua = userAgent.ToLowerInvariant();
        var info = new ClientInfo();

        // Detect Bot/Crawler
        var botPatterns = new[] { "bot", "crawler", "spider", "scraper", "facebookexternalhit", "twitterbot" };
        info.IsBot = botPatterns.Any(pattern => ua.Contains(pattern));

        // Detect Mobile
        var mobilePatterns = new[] { "mobile", "android", "iphone", "ipad", "ipod", "blackberry", "windows phone" };
        info.IsMobile = mobilePatterns.Any(pattern => ua.Contains(pattern));

        // Detect Device Type
        if (ua.Contains("tablet") || ua.Contains("ipad"))
        {
            info.DeviceType = "Tablet";
        }
        else if (info.IsMobile)
        {
            info.DeviceType = "Mobile";
        }
        else
        {
            info.DeviceType = "Desktop";
        }

        // Detect Browser
        if (ua.Contains("chrome") && !ua.Contains("edg"))
        {
            info.Browser = "Chrome";
            var versionMatch = Regex.Match(userAgent, @"chrome/([\d.]+)", RegexOptions.IgnoreCase);
            if (versionMatch.Success)
            {
                info.BrowserVersion = versionMatch.Groups[1].Value;
            }
        }
        else if (ua.Contains("firefox"))
        {
            info.Browser = "Firefox";
            var versionMatch = Regex.Match(userAgent, @"firefox/([\d.]+)", RegexOptions.IgnoreCase);
            if (versionMatch.Success)
            {
                info.BrowserVersion = versionMatch.Groups[1].Value;
            }
        }
        else if (ua.Contains("safari") && !ua.Contains("chrome"))
        {
            info.Browser = "Safari";
            var versionMatch = Regex.Match(userAgent, @"version/([\d.]+)", RegexOptions.IgnoreCase);
            if (versionMatch.Success)
            {
                info.BrowserVersion = versionMatch.Groups[1].Value;
            }
        }
        else if (ua.Contains("edg"))
        {
            info.Browser = "Edge";
            var versionMatch = Regex.Match(userAgent, @"edg(?:e|a|ios)?/([\d.]+)", RegexOptions.IgnoreCase);
            if (versionMatch.Success)
            {
                info.BrowserVersion = versionMatch.Groups[1].Value;
            }
        }
        else if (ua.Contains("opera") || ua.Contains("opr"))
        {
            info.Browser = "Opera";
            var versionMatch = Regex.Match(userAgent, @"(?:opera|opr)/([\d.]+)", RegexOptions.IgnoreCase);
            if (versionMatch.Success)
            {
                info.BrowserVersion = versionMatch.Groups[1].Value;
            }
        }
        else
        {
            info.Browser = "Unknown";
        }

        // Detect Operating System
        if (ua.Contains("windows"))
        {
            if (ua.Contains("windows nt 10.0"))
            {
                info.OperatingSystem = "Windows 10/11";
            }
            else if (ua.Contains("windows nt 6.3"))
            {
                info.OperatingSystem = "Windows 8.1";
            }
            else if (ua.Contains("windows nt 6.2"))
            {
                info.OperatingSystem = "Windows 8";
            }
            else if (ua.Contains("windows nt 6.1"))
            {
                info.OperatingSystem = "Windows 7";
            }
            else
            {
                info.OperatingSystem = "Windows";
            }
        }
        else if (ua.Contains("mac os x") || ua.Contains("macintosh"))
        {
            info.OperatingSystem = "macOS";
        }
        else if (ua.Contains("linux"))
        {
            info.OperatingSystem = "Linux";
        }
        else if (ua.Contains("android"))
        {
            var versionMatch = Regex.Match(userAgent, @"android ([\d.]+)", RegexOptions.IgnoreCase);
            if (versionMatch.Success)
            {
                info.OperatingSystem = $"Android {versionMatch.Groups[1].Value}";
            }
            else
            {
                info.OperatingSystem = "Android";
            }
        }
        else if (ua.Contains("iphone") || ua.Contains("ipad") || ua.Contains("ipod"))
        {
            var versionMatch = Regex.Match(userAgent, @"os ([\d_]+)", RegexOptions.IgnoreCase);
            if (versionMatch.Success)
            {
                var version = versionMatch.Groups[1].Value.Replace("_", ".");
                info.OperatingSystem = $"iOS {version}";
            }
            else
            {
                info.OperatingSystem = "iOS";
            }
        }
        else
        {
            info.OperatingSystem = "Unknown";
        }

        return info;
    }

    private class ClientInfo
    {
        public string? UserAgent { get; set; }
        public string? Browser { get; set; }
        public string? BrowserVersion { get; set; }
        public string? OperatingSystem { get; set; }
        public string? DeviceType { get; set; }
        public bool IsMobile { get; set; }
        public bool IsBot { get; set; }
    }
}

