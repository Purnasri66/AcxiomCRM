using AcxiomCRM.Data;
using AcxiomCRM.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Repositories;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Customer?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(c => c.Leads)
            .Include(c => c.Opportunities)
            .Include(c => c.FollowUps)
            .Include(c => c.Activities)
            .Include(c => c.CreatedByUser)
            .FirstOrDefaultAsync(c => c.CustomerId == id);
    }

    public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null)
    {
        return !await _dbSet.AnyAsync(c => c.Email == email && (!excludeId.HasValue || c.CustomerId != excludeId.Value));
    }

    public async Task<bool> IsPhoneUniqueAsync(string phone, int? excludeId = null)
    {
        return !await _dbSet.AnyAsync(c => c.Phone == phone && (!excludeId.HasValue || c.CustomerId != excludeId.Value));
    }

    public async Task<string> GenerateCustomerCodeAsync()
    {
        var maxCode = await _context.Customers.IgnoreQueryFilters()
            .OrderByDescending(c => c.CustomerId)
            .Select(c => c.CustomerCode)
            .FirstOrDefaultAsync();

        if (maxCode == null) return "CUST-0001";
        var num = int.Parse(maxCode.Replace("CUST-", ""));
        return $"CUST-{(num + 1):D4}";
    }

    public IQueryable<Customer> GetCustomersQuery(string? userId = null)
    {
        var query = _dbSet.AsQueryable();
        if (!string.IsNullOrEmpty(userId))
            query = query.Where(c => c.CreatedBy == userId);
        return query;
    }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Email == email);
    }
}
