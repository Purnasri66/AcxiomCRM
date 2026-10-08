using AcxiomCRM.Models.Entities;

namespace AcxiomCRM.Repositories;

public interface IFollowUpRepository : IRepository<FollowUp>
{
    Task<FollowUp?> GetByIdWithDetailsAsync(int id);
    IQueryable<FollowUp> GetFollowUpsQuery(string? userId = null);
    Task<IEnumerable<FollowUp>> GetOverdueFollowUpsAsync(string? userId = null);
    Task<IEnumerable<FollowUp>> GetUpcomingFollowUpsAsync(string? userId = null, int days = 7);
}
