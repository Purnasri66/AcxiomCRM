using AcxiomCRM.Data;
using AcxiomCRM.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Repositories;

public class ActivityRepository : Repository<Activity>, IActivityRepository
{
    public ActivityRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Activity?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(a => a.Customer)
            .Include(a => a.Lead)
            .Include(a => a.AssignedUser)
            .FirstOrDefaultAsync(a => a.ActivityId == id);
    }

    public IQueryable<Activity> GetActivitiesQuery(string? userId = null)
    {
        var query = _dbSet
            .Include(a => a.Customer)
            .Include(a => a.Lead)
            .Include(a => a.AssignedUser)
            .AsQueryable();
        if (!string.IsNullOrEmpty(userId))
            query = query.Where(a => a.AssignedTo == userId);
        return query;
    }
}
