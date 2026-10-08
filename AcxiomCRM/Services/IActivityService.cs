using AcxiomCRM.Models.Entities;

namespace AcxiomCRM.Services;

/// <summary>
/// Service for managing activities (Call, Meeting, Email, Task).
/// </summary>
public interface IActivityService
{
    Task<Activity?> GetByIdAsync(int id, string userId, string role);
    Task<Activity?> GetByIdWithDetailsAsync(int id, string userId, string role);
    Task<(IEnumerable<Activity> Items, int TotalCount)> GetPagedAsync(string userId, string role, string? typeFilter = null, int page = 1, int pageSize = 10, string? sortBy = null, bool ascending = true);
    Task<(bool Success, string? Error, Activity? Activity)> CreateAsync(Activity activity, string userId, string role);
    Task<(bool Success, string? Error)> UpdateAsync(Activity activity, string userId, string role);
    Task<(bool Success, string? Error)> DeleteAsync(int id, string userId, string role);
}
