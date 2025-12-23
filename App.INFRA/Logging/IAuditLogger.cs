namespace App.INFRA.Logging;

/// <summary>
/// Interface for audit logging operations.
/// </summary>
public interface IAuditLogger
{
    /// <summary>
    /// Log an audit event.
    /// </summary>
    void Log(AuditLogEntry entry);
    
    /// <summary>
    /// Log an audit event asynchronously.
    /// </summary>
    Task LogAsync(AuditLogEntry entry, CancellationToken ct = default);
    
    /// <summary>
    /// Log an action with automatic user context enrichment.
    /// </summary>
    void LogAction(string action, string entityType, string? entityId = null, 
        object? oldValues = null, object? newValues = null, 
        Dictionary<string, object>? additionalData = null);
    
    /// <summary>
    /// Log an action asynchronously with automatic user context enrichment.
    /// </summary>
    Task LogActionAsync(string action, string entityType, string? entityId = null, 
        object? oldValues = null, object? newValues = null, 
        Dictionary<string, object>? additionalData = null, 
        CancellationToken ct = default);
}
