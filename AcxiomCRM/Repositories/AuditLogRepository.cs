using AcxiomCRM.Data;
using AcxiomCRM.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Repositories;

public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(ApplicationDbContext context) : base(context) { }

    public IQueryable<AuditLog> GetAuditLogsQuery()
    {
        return _dbSet.OrderByDescending(a => a.CreatedDate).AsQueryable();
    }

    public async Task<IEnumerable<AuditLog>> GetRecentAuditLogsAsync(int count = 20)
    {
        return await _dbSet.OrderByDescending(a => a.CreatedDate).Take(count).ToListAsync();
    }
}
