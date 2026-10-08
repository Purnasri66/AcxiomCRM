using AcxiomCRM.Models.Entities;

namespace AcxiomCRM.Services;

/// <summary>
/// Service for managing leads, including status workflow and conversion.
/// </summary>
public interface ILeadService
{
    Task<Lead?> GetByIdAsync(int id, string userId, string role);
    Task<Lead?> GetByIdWithDetailsAsync(int id, string userId, string role);
    Task<(IEnumerable<Lead> Items, int TotalCount)> GetPagedAsync(string userId, string role, string? search = null, string? statusFilter = null, int page = 1, int pageSize = 10, string? sortBy = null, bool ascending = true);
    Task<(bool Success, string? Error, Lead? Lead)> CreateAsync(Lead lead, string userId, string role);
    Task<(bool Success, string? Error)> UpdateAsync(Lead lead, string userId, string role);
    Task<(bool Success, string? Error)> DeleteAsync(int id, string userId, string role);
    /// <summary>Changes lead status following the valid transition map.</summary>
    Task<(bool Success, string? Error)> ChangeStatusAsync(int id, string newStatus, string userId, string role);
    /// <summary>Converts a Qualified lead to a Customer + optional Opportunity.</summary>
    Task<(bool Success, string? Error, int? CustomerId)> ConvertAsync(int leadId, string? opportunityName, decimal? amount, string userId, string role);
    /// <summary>Gets valid next statuses for a given current status.</summary>
    IEnumerable<string> GetValidTransitions(string currentStatus);
}
