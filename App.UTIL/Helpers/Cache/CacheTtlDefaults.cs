using System;

namespace App.UTIL.Helpers.Cache;

/// <summary>
/// Centralized TTL defaults for all cache schemas.
/// Provides consistent expiration times across the application.
/// </summary>
public static class CacheTtlDefaults
{
    #region --- Short-lived (Hot data, frequently changing) ---
    
    /// <summary>1 minute - for very volatile data</summary>
    public static readonly TimeSpan ExtraShort = TimeSpan.FromMinutes(1);
    
    /// <summary>5 minutes - for lists, search results</summary>
    public static readonly TimeSpan Short = TimeSpan.FromMinutes(5);
    
    #endregion
    
    #region --- Medium (Balanced) ---
    
    /// <summary>10 minutes - for detail pages</summary>
    public static readonly TimeSpan Medium = TimeSpan.FromMinutes(10);
    
    /// <summary>30 minutes - for filters, categories</summary>
    public static readonly TimeSpan MediumLong = TimeSpan.FromMinutes(30);
    
    #endregion
    
    #region --- Long-lived (Static or rarely changing) ---
    
    /// <summary>1 hour - for configuration-like data</summary>
    public static readonly TimeSpan Long = TimeSpan.FromHours(1);
    
    /// <summary>6 hours - for static content</summary>
    public static readonly TimeSpan VeryLong = TimeSpan.FromHours(6);
    
    /// <summary>24 hours - for nearly static data</summary>
    public static readonly TimeSpan Day = TimeSpan.FromHours(24);
    
    #endregion
    
    #region --- Auth/Session ---
    
    /// <summary>15 minutes - for access tokens cache</summary>
    public static readonly TimeSpan AccessToken = TimeSpan.FromMinutes(15);
    
    /// <summary>7 days - for refresh tokens cache</summary>
    public static readonly TimeSpan RefreshToken = TimeSpan.FromDays(7);
    
    /// <summary>30 minutes - for session data</summary>
    public static readonly TimeSpan Session = TimeSpan.FromMinutes(30);
    
    #endregion
}
