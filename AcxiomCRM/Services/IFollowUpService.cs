using AcxiomCRM.Models.Entities;

namespace AcxiomCRM.Services;

/// <summary>
/// Service for managing follow-ups including scheduling, completion and rescheduling.
/// </summary>
public interface IFollowUpService
{
    Task<FollowUp?> GetByIdAsync(int id, string userId, string role);
    Task<FollowUp?> GetByIdWithDetailsAsync(int id, string userId, string role);
    Task<(IEnumerable<FollowUp> Items, int TotalCount)> GetPagedAsync(string userId, string role, string? statusFilter = null, int page = 1, int pageSize = 10, string? sortBy = null, bool ascending = true);
    Task<(bool Success, string? Error, FollowUp? FollowUp)> CreateAsync(FollowUp followUp, string userId, string role);
    Task<(bool Success, string? Error)> UpdateAsync(FollowUp followUp, string userId, string role);
    Task<(bool Success, string? Error)> CompleteAsync(int id, string? remarks, string userId, string role);
    Task<(bool Success, string? Error)> RescheduleAsync(int id, DateTime newDate, string userId, string role);
    Task<(bool Success, string? Error)> CancelAsync(int id, string userId, string role);
    Task<IEnumerable<FollowUp>> GetOverdueAsync(string userId, string role);
    Task<IEnumerable<FollowUp>> GetUpcomingAsync(string userId, string role, int days = 7);
    Task<int> GetOverdueCountAsync(string userId, string role);
}
