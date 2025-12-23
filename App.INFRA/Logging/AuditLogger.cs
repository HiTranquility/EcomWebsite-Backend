using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace App.INFRA.Logging;

/// <summary>
/// Implementation of IAuditLogger that logs audit events to the configured logger.
/// Automatically enriches entries with user context from HttpContext.
/// </summary>
public class AuditLogger : IAuditLogger
{
    private readonly ILogger<AuditLogger> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public AuditLogger(ILogger<AuditLogger> logger, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }
    
    /// <inheritdoc />
    public void Log(AuditLogEntry entry)
    {
        var enrichedEntry = EnrichEntry(entry);
        
        _logger.LogInformation(
            "AUDIT: {Action} on {EntityType}:{EntityId} by User:{UserId} from IP:{IpAddress}",
            enrichedEntry.Action,
            enrichedEntry.EntityType,
            enrichedEntry.EntityId ?? "N/A",
            enrichedEntry.UserId ?? "Anonymous",
            enrichedEntry.IpAddress ?? "Unknown");
        
        // Log full entry as structured data for later analysis
        _logger.LogDebug("AUDIT_DETAIL: {AuditEntry}", 
            JsonSerializer.Serialize(enrichedEntry, new JsonSerializerOptions { WriteIndented = false }));
    }
    
    /// <inheritdoc />
    public Task LogAsync(AuditLogEntry entry, CancellationToken ct = default)
    {
        Log(entry);
        return Task.CompletedTask;
    }
    
    /// <inheritdoc />
    public void LogAction(string action, string entityType, string? entityId = null, 
        object? oldValues = null, object? newValues = null, 
        Dictionary<string, object>? additionalData = null)
    {
        var entry = new AuditLogEntry(
            Timestamp: DateTime.UtcNow,
            UserId: null,
            UserEmail: null,
            Action: action,
            EntityType: entityType,
            EntityId: entityId,
            OldValues: oldValues,
            NewValues: newValues,
            AdditionalData: additionalData);
        
        Log(entry);
    }
    
    /// <inheritdoc />
    public Task LogActionAsync(string action, string entityType, string? entityId = null, 
        object? oldValues = null, object? newValues = null, 
        Dictionary<string, object>? additionalData = null, 
        CancellationToken ct = default)
    {
        LogAction(action, entityType, entityId, oldValues, newValues, additionalData);
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Enrich the audit entry with user context from HttpContext.
    /// </summary>
    private AuditLogEntry EnrichEntry(AuditLogEntry entry)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return entry;
        
        var userId = entry.UserId ?? GetUserId(httpContext);
        var userEmail = entry.UserEmail ?? GetUserEmail(httpContext);
        var ipAddress = entry.IpAddress ?? GetIpAddress(httpContext);
        var userAgent = entry.UserAgent ?? httpContext.Request.Headers.UserAgent.ToString();
        
        return entry with 
        { 
            UserId = userId,
            UserEmail = userEmail,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };
    }
    
    private static string? GetUserId(HttpContext context)
    {
        return context.User?.FindFirst("sub")?.Value 
            ?? context.User?.FindFirst("id")?.Value
            ?? context.User?.Identity?.Name;
    }
    
    private static string? GetUserEmail(HttpContext context)
    {
        return context.User?.FindFirst("email")?.Value;
    }
    
    private static string? GetIpAddress(HttpContext context)
    {
        // Check for forwarded headers first (behind proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }
        
        return context.Connection.RemoteIpAddress?.ToString();
    }
}
