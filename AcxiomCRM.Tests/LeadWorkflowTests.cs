using AcxiomCRM.Data;
using AcxiomCRM.Models.Entities;
using AcxiomCRM.Repositories;
using AcxiomCRM.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AcxiomCRM.Tests;

public class LeadWorkflowTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task ChangeStatus_AllowsValidTransitions()
    {
        var context = CreateInMemoryDbContext();
        var leadRepo = new LeadRepository(context);
        var customerRepo = new Mock<ICustomerRepository>();
        var oppRepo = new Mock<IOpportunityRepository>();
        var auditService = new Mock<IAuditService>();
        var scoringService = new LeadScoringService();

        var lead = new Lead
        {
            LeadId = 1,
            LeadCode = "LEAD-0001",
            LeadName = "Test Lead",
            Email = "test@lead.com",
            Phone = "9876543210",
            Source = "Website",
            Status = "New",
            AssignedTo = "user-1"
        };
        await leadRepo.AddAsync(lead);
        await leadRepo.SaveChangesAsync();

        var service = new LeadService(leadRepo, customerRepo.Object, oppRepo.Object, auditService.Object, scoringService);

        // New -> Contacted (Valid)
        var (s1, err1) = await service.ChangeStatusAsync(1, "Contacted", "user-1", "SalesExecutive");
        Assert.True(s1);

        // Contacted -> Qualified (Valid)
        var (s2, err2) = await service.ChangeStatusAsync(1, "Qualified", "user-1", "SalesExecutive");
        Assert.True(s2);
    }

    [Fact]
    public async Task ChangeStatus_RejectsInvalidTransitions()
    {
        var context = CreateInMemoryDbContext();
        var leadRepo = new LeadRepository(context);
        var customerRepo = new Mock<ICustomerRepository>();
        var oppRepo = new Mock<IOpportunityRepository>();
        var auditService = new Mock<IAuditService>();
        var scoringService = new LeadScoringService();

        var lead = new Lead
        {
            LeadId = 2,
            LeadCode = "LEAD-0002",
            LeadName = "Test Lead 2",
            Email = "test2@lead.com",
            Phone = "9876543211",
            Source = "Website",
            Status = "New",
            AssignedTo = "user-1"
        };
        await leadRepo.AddAsync(lead);
        await leadRepo.SaveChangesAsync();

        var service = new LeadService(leadRepo, customerRepo.Object, oppRepo.Object, auditService.Object, scoringService);

        // New -> Converted (Invalid, must go through Contacted and Qualified first)
        var (success, error) = await service.ChangeStatusAsync(2, "Converted", "user-1", "SalesExecutive");
        Assert.False(success);
        Assert.Contains("Cannot transition", error);
    }
}
