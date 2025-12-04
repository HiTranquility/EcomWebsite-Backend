using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.Configurations;

public static class SessionConfig
{
    private const string DefaultCookieName = ".EcomWebsite.Session";

    public static IServiceCollection ConfigureSession(this IServiceCollection services, IConfiguration configuration)
    {
        var sessionSection = configuration.GetSection("Session");
        var idleTimeoutMinutes = sessionSection.GetValue<int?>("IdleTimeoutMinutes") ?? 30;
        var cookieName = sessionSection.GetValue<string>("CookieName");
        var requireConsent = sessionSection.GetValue<bool?>("RequireConsent") ?? false;
        var sameSite = sessionSection.GetValue<string>("SameSite") ?? "None";
        var securePolicySetting = sessionSection.GetValue<string>("SecurePolicy") ?? "Always";
        var isEssential = sessionSection.GetValue<bool?>("IsEssential") ?? true;

        var sameSiteMode = sameSite.ToUpperInvariant() switch
        {
            "STRICT" => SameSiteMode.Strict,
            "LAX" => SameSiteMode.Lax,
            "NONE" => SameSiteMode.None,
            _ => SameSiteMode.None
        };

        var securePolicy = securePolicySetting.ToUpperInvariant() switch
        {
            "ALWAYS" => CookieSecurePolicy.Always,
            "NONE" => CookieSecurePolicy.None,
            "SAMEASREQUEST" => CookieSecurePolicy.SameAsRequest,
            _ => CookieSecurePolicy.Always
        };

        services.AddDistributedMemoryCache();

        services.Configure<CookiePolicyOptions>(options =>
        {
            options.CheckConsentNeeded = _ => requireConsent;
            options.MinimumSameSitePolicy = sameSiteMode;
            options.HttpOnly = HttpOnlyPolicy.Always;
            options.Secure = securePolicy;
        });

        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(idleTimeoutMinutes);
            options.Cookie.Name = string.IsNullOrWhiteSpace(cookieName) ? DefaultCookieName : cookieName;
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = isEssential;
            options.Cookie.SameSite = sameSiteMode;
            options.Cookie.SecurePolicy = securePolicy;
        });

        return services;
    }
}
