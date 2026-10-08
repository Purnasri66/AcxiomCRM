using AcxiomCRM.Models.Entities;

namespace AcxiomCRM.Repositories;

public interface IOpportunityRepository : IRepository<Opportunity>
{
    Task<Opportunity?> GetByIdWithDetailsAsync(int id);
    IQueryable<Opportunity> GetOpportunitiesQuery(string? userId = null);
}
