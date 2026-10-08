using AcxiomCRM.Models.Entities;

namespace AcxiomCRM.Services;

/// <summary>
/// Service for managing customer entities with business logic and scope enforcement.
/// </summary>
public interface ICustomerService
{
    /// <summary>Gets a customer by ID, respecting role scope.</summary>
    Task<Customer?> GetByIdAsync(int id, string userId, string role);
    /// <summary>Gets customer with all related data for the 360 view.</summary>
    Task<Customer?> GetByIdWithDetailsAsync(int id, string userId, string role);
    /// <summary>Gets paged and filtered customers, scoped by role.</summary>
    Task<(IEnumerable<Customer> Items, int TotalCount)> GetPagedAsync(string userId, string role, string? search = null, int page = 1, int pageSize = 10, string? sortBy = null, bool ascending = true);
    /// <summary>Creates a new customer with validation.</summary>
    Task<(bool Success, string? Error, Customer? Customer)> CreateAsync(Customer customer, string userId, string role);
    /// <summary>Updates an existing customer with validation.</summary>
    Task<(bool Success, string? Error)> UpdateAsync(Customer customer, string userId, string role);
    /// <summary>Soft-deletes a customer.</summary>
    Task<(bool Success, string? Error)> DeleteAsync(int id, string userId, string role);
    /// <summary>Gets all active customers for dropdown lists.</summary>
    Task<IEnumerable<Customer>> GetAllActiveAsync(string userId, string role);
}
