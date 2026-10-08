namespace AcxiomCRM.Services;

/// <summary>
/// Service for creating audit log entries. All state changes must be recorded.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs an audit entry with old and new values for diff viewing.
    /// </summary>
    Task LogAsync(string userId, string action, string entityName, string? recordId = null, 
                  object? oldValue = null, object? newValue = null, string? ipAddress = null);
    
    /// <summary>
    /// Logs a simple action without value tracking.
    /// </summary>
    Task LogActionAsync(string userId, string action, string entityName, string? recordId = null, string? ipAddress = null);
}
