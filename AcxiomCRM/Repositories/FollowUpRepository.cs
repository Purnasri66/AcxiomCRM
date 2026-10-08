using AcxiomCRM.Data;
using AcxiomCRM.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Repositories;

public class FollowUpRepository : Repository<FollowUp>, IFollowUpRepository
{
    public FollowUpRepository(ApplicationDbContext context) : base(context) { }

    public async Task<FollowUp?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(f => f.Customer)
            .Include(f => f.Lead)
            .Include(f => f.Opportunity)
            .Include(f => f.AssignedUser)
            .FirstOrDefaultAsync(f => f.FollowUpId == id);
    }

    public IQueryable<FollowUp> GetFollowUpsQuery(string? userId = null)
    {
        var query = _dbSet
            .Include(f => f.Customer)
            .Include(f => f.Lead)
            .Include(f => f.Opportunity)
            .Include(f => f.AssignedUser)
            .AsQueryable();
        if (!string.IsNullOrEmpty(userId))
            query = query.Where(f => f.AssignedTo == userId);
        return query;
    }

    public async Task<IEnumerable<FollowUp>> GetOverdueFollowUpsAsync(string? userId = null)
    {
        var query = _dbSet
            .Include(f => f.Customer)
            .Include(f => f.Lead)
            .Include(f => f.Opportunity)
            .Where(f => f.Status == "Planned" && f.FollowUpDate < DateTime.UtcNow);
        if (!string.IsNullOrEmpty(userId))
            query = query.Where(f => f.AssignedTo == userId);
        return await query.OrderBy(f => f.FollowUpDate).ToListAsync();
    }

    public async Task<IEnumerable<FollowUp>> GetUpcomingFollowUpsAsync(string? userId = null, int days = 7)
    {
        var endDate = DateTime.UtcNow.AddDays(days);
        var query = _dbSet
            .Include(f => f.Customer)
            .Include(f => f.Lead)
            .Include(f => f.Opportunity)
            .Where(f => f.Status == "Planned" && f.FollowUpDate >= DateTime.UtcNow && f.FollowUpDate <= endDate);
        if (!string.IsNullOrEmpty(userId))
            query = query.Where(f => f.AssignedTo == userId);
        return await query.OrderBy(f => f.FollowUpDate).ToListAsync();
    }
}
