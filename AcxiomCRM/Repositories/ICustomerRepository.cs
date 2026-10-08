using AcxiomCRM.Models.Entities;

namespace AcxiomCRM.Repositories;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByIdWithDetailsAsync(int id);
    Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null);
    Task<bool> IsPhoneUniqueAsync(string phone, int? excludeId = null);
    Task<string> GenerateCustomerCodeAsync();
    IQueryable<Customer> GetCustomersQuery(string? userId = null);
    Task<Customer?> GetByEmailAsync(string email);
}
