using AcxiomCRM.Models.Entities;
using AcxiomCRM.Repositories;
using Newtonsoft.Json;

namespace AcxiomCRM.Services;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task LogAsync(string userId, string action, string entityName, string? recordId = null,
                               object? oldValue = null, object? newValue = null, string? ipAddress = null)
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = entityName,
            RecordId = recordId,
            OldValue = oldValue != null ? SanitizeAndSerialize(oldValue) : null,
            NewValue = newValue != null ? SanitizeAndSerialize(newValue) : null,
            CreatedDate = DateTime.UtcNow,
            IpAddress = ipAddress
        };

        await _auditLogRepository.AddAsync(auditLog);
        await _auditLogRepository.SaveChangesAsync();
    }

    public async Task LogActionAsync(string userId, string action, string entityName, string? recordId = null, string? ipAddress = null)
    {
        await LogAsync(userId, action, entityName, recordId, ipAddress: ipAddress);
    }

    private static string SanitizeAndSerialize(object value)
    {
        var json = JsonConvert.SerializeObject(value, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        });

        // Remove sensitive fields
        var sensitiveFields = new[] { "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "Token", "Secret" };
        foreach (var field in sensitiveFields)
        {
            json = System.Text.RegularExpressions.Regex.Replace(
                json, $"\"{field}\"\\s*:\\s*\"[^\"]*\"", $"\"{field}\":\"[REDACTED]\"");
        }

        return json;
    }
}
