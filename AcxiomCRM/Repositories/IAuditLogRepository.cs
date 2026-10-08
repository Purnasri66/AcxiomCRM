using AcxiomCRM.Models.Entities;

namespace AcxiomCRM.Repositories;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    IQueryable<AuditLog> GetAuditLogsQuery();
    Task<IEnumerable<AuditLog>> GetRecentAuditLogsAsync(int count = 20);
}
