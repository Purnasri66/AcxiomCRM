using AcxiomCRM.Data;
using AcxiomCRM.Models.Entities;
using AcxiomCRM.Repositories;
using AcxiomCRM.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AcxiomCRM.Tests;

public class DuplicateCustomerTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateCustomer_PreventsDuplicateEmail()
    {
        var context = CreateInMemoryDbContext();
        var customerRepo = new CustomerRepository(context);
        var auditService = new Mock<IAuditService>();
        var service = new CustomerService(customerRepo, auditService.Object);

        var c1 = new Customer
        {
            CustomerName = "First Customer",
            Email = "unique@test.com",
            Phone = "9876543210",
            Status = "Active"
        };
        var (s1, _, _) = await service.CreateAsync(c1, "admin-1", "Admin");
        Assert.True(s1);

        var c2 = new Customer
        {
            CustomerName = "Duplicate Customer",
            Email = "unique@test.com", // Duplicate
            Phone = "9876543211",
            Status = "Active"
        };
        var (s2, err2, _) = await service.CreateAsync(c2, "admin-1", "Admin");
        Assert.False(s2);
        Assert.Contains("already exists", err2);
    }

    [Fact]
    public async Task CreateCustomer_PreventsDuplicatePhone()
    {
        var context = CreateInMemoryDbContext();
        var customerRepo = new CustomerRepository(context);
        var auditService = new Mock<IAuditService>();
        var service = new CustomerService(customerRepo, auditService.Object);

        var c1 = new Customer
        {
            CustomerName = "First Customer",
            Email = "email1@test.com",
            Phone = "9876543210",
            Status = "Active"
        };
        await service.CreateAsync(c1, "admin-1", "Admin");

        var c2 = new Customer
        {
            CustomerName = "Duplicate Phone Customer",
            Email = "email2@test.com",
            Phone = "9876543210", // Duplicate phone
            Status = "Active"
        };
        var (s2, err2, _) = await service.CreateAsync(c2, "admin-1", "Admin");
        Assert.False(s2);
        Assert.Contains("already exists", err2);
    }
}
