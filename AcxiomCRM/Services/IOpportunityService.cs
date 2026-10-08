using AcxiomCRM.Models.Entities;

namespace AcxiomCRM.Services;

/// <summary>
/// Service for managing opportunities, pipeline stages and deal health.
/// </summary>
public interface IOpportunityService
{
    Task<Opportunity?> GetByIdAsync(int id, string userId, string role);
    Task<Opportunity?> GetByIdWithDetailsAsync(int id, string userId, string role);
    Task<(IEnumerable<Opportunity> Items, int TotalCount)> GetPagedAsync(string userId, string role, string? search = null, string? stageFilter = null, int page = 1, int pageSize = 10, string? sortBy = null, bool ascending = true);
    Task<(bool Success, string? Error, Opportunity? Opportunity)> CreateAsync(Opportunity opportunity, string userId, string role);
    Task<(bool Success, string? Error)> UpdateAsync(Opportunity opportunity, string userId, string role);
    Task<(bool Success, string? Error)> DeleteAsync(int id, string userId, string role);
    /// <summary>Changes the pipeline stage of an opportunity (used by Kanban drag-and-drop).</summary>
    Task<(bool Success, string? Error)> ChangeStageAsync(int id, string newStage, string userId, string role);
    /// <summary>Gets all opportunities grouped by stage for the Kanban board.</summary>
    Task<Dictionary<string, List<Opportunity>>> GetKanbanDataAsync(string userId, string role);
    /// <summary>Gets opportunities at risk (low health score).</summary>
    Task<IEnumerable<Opportunity>> GetAtRiskDealsAsync(string userId, string role, int count = 5);
}
