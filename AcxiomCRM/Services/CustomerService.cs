using AcxiomCRM.Models.Entities;
using AcxiomCRM.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepo;
    private readonly IAuditService _auditService;

    public CustomerService(ICustomerRepository customerRepo, IAuditService auditService)
    {
        _customerRepo = customerRepo;
        _auditService = auditService;
    }

    public async Task<Customer?> GetByIdAsync(int id, string userId, string role)
    {
        var customer = await _customerRepo.GetByIdAsync(id);
        if (customer == null) return null;
        if (role == "SalesExecutive" && customer.CreatedBy != userId) return null;
        return customer;
    }

    public async Task<Customer?> GetByIdWithDetailsAsync(int id, string userId, string role)
    {
        var customer = await _customerRepo.GetByIdWithDetailsAsync(id);
        if (customer == null) return null;
        if (role == "SalesExecutive" && customer.CreatedBy != userId) return null;
        return customer;
    }

    public async Task<(IEnumerable<Customer> Items, int TotalCount)> GetPagedAsync(
        string userId, string role, string? search = null, int page = 1, int pageSize = 10, string? sortBy = null, bool ascending = true)
    {
        var scopedUserId = role == "SalesExecutive" ? userId : null;
        var query = _customerRepo.GetCustomersQuery(scopedUserId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(c =>
                c.CustomerName.ToLower().Contains(s) ||
                c.Email.ToLower().Contains(s) ||
                c.Phone.Contains(s) ||
                (c.CompanyName != null && c.CompanyName.ToLower().Contains(s)));
        }

        var totalCount = await query.CountAsync();

        query = sortBy?.ToLower() switch
        {
            "name" => ascending ? query.OrderBy(c => c.CustomerName) : query.OrderByDescending(c => c.CustomerName),
            "email" => ascending ? query.OrderBy(c => c.Email) : query.OrderByDescending(c => c.Email),
            "company" => ascending ? query.OrderBy(c => c.CompanyName) : query.OrderByDescending(c => c.CompanyName),
            "date" => ascending ? query.OrderBy(c => c.CreatedDate) : query.OrderByDescending(c => c.CreatedDate),
            "status" => ascending ? query.OrderBy(c => c.Status) : query.OrderByDescending(c => c.Status),
            _ => query.OrderByDescending(c => c.CreatedDate)
        };

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, totalCount);
    }

    public async Task<(bool Success, string? Error, Customer? Customer)> CreateAsync(Customer customer, string userId, string role)
    {
        if (!await _customerRepo.IsEmailUniqueAsync(customer.Email))
            return (false, "A customer with this email already exists.", null);

        if (!await _customerRepo.IsPhoneUniqueAsync(customer.Phone))
            return (false, "A customer with this phone number already exists.", null);

        customer.CustomerCode = await _customerRepo.GenerateCustomerCodeAsync();
        customer.CreatedBy = userId;
        customer.CreatedDate = DateTime.UtcNow;

        await _customerRepo.AddAsync(customer);
        await _customerRepo.SaveChangesAsync();

        await _auditService.LogAsync(userId, "Create", "Customer", customer.CustomerId.ToString(), newValue: new
        {
            customer.CustomerCode, customer.CustomerName, customer.Email, customer.Phone, customer.CompanyName, customer.Status
        });

        return (true, null, customer);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(Customer customer, string userId, string role)
    {
        var existing = await _customerRepo.GetByIdAsync(customer.CustomerId);
        if (existing == null) return (false, "Customer not found.");
        if (role == "SalesExecutive" && existing.CreatedBy != userId) return (false, "Access denied.");

        if (!await _customerRepo.IsEmailUniqueAsync(customer.Email, customer.CustomerId))
            return (false, "A customer with this email already exists.");

        if (!await _customerRepo.IsPhoneUniqueAsync(customer.Phone, customer.CustomerId))
            return (false, "A customer with this phone number already exists.");

        var oldValue = new { existing.CustomerName, existing.Email, existing.Phone, existing.CompanyName, existing.Status };

        existing.CustomerName = customer.CustomerName;
        existing.Email = customer.Email;
        existing.Phone = customer.Phone;
        existing.CompanyName = customer.CompanyName;
        existing.Address = customer.Address;
        existing.City = customer.City;
        existing.State = customer.State;
        existing.Status = customer.Status;
        existing.ModifiedDate = DateTime.UtcNow;

        _customerRepo.Update(existing);
        await _customerRepo.SaveChangesAsync();

        await _auditService.LogAsync(userId, "Update", "Customer", customer.CustomerId.ToString(),
            oldValue: oldValue,
            newValue: new { existing.CustomerName, existing.Email, existing.Phone, existing.CompanyName, existing.Status });

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id, string userId, string role)
    {
        var existing = await _customerRepo.GetByIdAsync(id);
        if (existing == null) return (false, "Customer not found.");
        if (role == "SalesExecutive" && existing.CreatedBy != userId) return (false, "Access denied.");

        existing.IsDeleted = true;
        existing.IsActive = false;
        existing.ModifiedDate = DateTime.UtcNow;
        _customerRepo.Update(existing);
        await _customerRepo.SaveChangesAsync();

        await _auditService.LogAsync(userId, "Delete", "Customer", id.ToString(),
            oldValue: new { existing.CustomerName, existing.Status },
            newValue: new { Status = "Deleted" });

        return (true, null);
    }

    public async Task<IEnumerable<Customer>> GetAllActiveAsync(string userId, string role)
    {
        var scopedUserId = role == "SalesExecutive" ? userId : null;
        return await _customerRepo.GetCustomersQuery(scopedUserId)
            .Where(c => c.IsActive)
            .OrderBy(c => c.CustomerName)
            .ToListAsync();
    }
}
