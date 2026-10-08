using AcxiomCRM.Data;
using AcxiomCRM.Models.Entities;
using AcxiomCRM.Repositories;
using AcxiomCRM.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AcxiomCRM.Tests;

public class ScopeFilteringTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task SalesExecutive_CannotAccessOtherUsersCustomer()
    {
        var context = CreateInMemoryDbContext();
        var customerRepo = new CustomerRepository(context);
        var auditService = new Mock<IAuditService>();
        var service = new CustomerService(customerRepo, auditService.Object);

        var c1 = new Customer
        {
            CustomerId = 1,
            CustomerCode = "CUST-0001",
            CustomerName = "Sales2 Customer",
            Email = "sales2@client.com",
            Phone = "9876543210",
            CreatedBy = "sales2-id",
            Status = "Active"
        };
        await customerRepo.AddAsync(c1);
        await customerRepo.SaveChangesAsync();

        // sales1 attempts to access sales2's customer
        var result = await service.GetByIdAsync(1, "sales1-id", "SalesExecutive");
        Assert.Null(result); // Forbidden / filtered out

        // Manager can access it
        var managerResult = await service.GetByIdAsync(1, "manager-id", "Manager");
        Assert.NotNull(managerResult);

        // Admin can access it
        var adminResult = await service.GetByIdAsync(1, "admin-id", "Admin");
        Assert.NotNull(adminResult);
    }
}
