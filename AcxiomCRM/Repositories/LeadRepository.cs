using AcxiomCRM.Data;
using AcxiomCRM.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Repositories;

public class LeadRepository : Repository<Lead>, ILeadRepository
{
    public LeadRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Lead?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(l => l.Opportunities)
            .Include(l => l.FollowUps)
            .Include(l => l.Activities)
            .Include(l => l.AssignedUser)
            .Include(l => l.ConvertedCustomer)
            .FirstOrDefaultAsync(l => l.LeadId == id);
    }

    public async Task<string> GenerateLeadCodeAsync()
    {
        var maxCode = await _context.Leads.IgnoreQueryFilters()
            .OrderByDescending(l => l.LeadId)
            .Select(l => l.LeadCode)
            .FirstOrDefaultAsync();

        if (maxCode == null) return "LEAD-0001";
        var num = int.Parse(maxCode.Replace("LEAD-", ""));
        return $"LEAD-{(num + 1):D4}";
    }

    public IQueryable<Lead> GetLeadsQuery(string? userId = null)
    {
        var query = _dbSet.Include(l => l.AssignedUser).AsQueryable();
        if (!string.IsNullOrEmpty(userId))
            query = query.Where(l => l.AssignedTo == userId);
        return query;
    }
}
