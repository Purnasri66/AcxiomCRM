using AcxiomCRM.Models.Entities;

namespace AcxiomCRM.Repositories;

public interface ILeadRepository : IRepository<Lead>
{
    Task<Lead?> GetByIdWithDetailsAsync(int id);
    Task<string> GenerateLeadCodeAsync();
    IQueryable<Lead> GetLeadsQuery(string? userId = null);
}
