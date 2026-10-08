using AcxiomCRM.Models.Entities;

namespace AcxiomCRM.Repositories;

public interface IActivityRepository : IRepository<Activity>
{
    Task<Activity?> GetByIdWithDetailsAsync(int id);
    IQueryable<Activity> GetActivitiesQuery(string? userId = null);
}
