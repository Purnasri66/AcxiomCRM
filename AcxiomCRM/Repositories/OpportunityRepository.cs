using AcxiomCRM.Data;
using AcxiomCRM.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Repositories;

public class OpportunityRepository : Repository<Opportunity>, IOpportunityRepository
{
    public OpportunityRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Opportunity?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(o => o.Customer)
            .Include(o => o.SourceLead)
            .Include(o => o.FollowUps)
            .Include(o => o.Activities)
            .Include(o => o.AssignedUser)
            .FirstOrDefaultAsync(o => o.OpportunityId == id);
    }

    public IQueryable<Opportunity> GetOpportunitiesQuery(string? userId = null)
    {
        var query = _dbSet
            .Include(o => o.Customer)
            .Include(o => o.AssignedUser)
            .AsQueryable();
        if (!string.IsNullOrEmpty(userId))
            query = query.Where(o => o.AssignedTo == userId);
        return query;
    }
}
