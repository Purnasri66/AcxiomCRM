using AcxiomCRM.Data;
using AcxiomCRM.Models.Entities;
using AcxiomCRM.Repositories;
using AcxiomCRM.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AcxiomCRM.Tests;

public class DealHealthScoreTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CalculateHealthScore_ReturnsExpectedRangeAndLabels()
    {
        var context = CreateInMemoryDbContext();
        var followUpRepo = new FollowUpRepository(context);
        var activityRepo = new ActivityRepository(context);
        var service = new DealHealthService(followUpRepo, activityRepo);

        var oppHealthy = new Opportunity
        {
            OpportunityId = 1,
            OpportunityName = "High Health Deal",
            CustomerId = 1,
            Amount = 1000000,
            Stage = "Negotiation",
            Probability = 80,
            ExpectedCloseDate = DateTime.UtcNow.AddDays(70),
            Status = "Open"
        };

        // Add recent activity
        await activityRepo.AddAsync(new Activity
        {
            ActivityType = "Meeting",
            Subject = "Demo",
            ActivityDate = DateTime.UtcNow.AddDays(-1),
            CustomerId = 1,
            AssignedTo = "user-1",
            Status = "Completed"
        });
        await activityRepo.SaveChangesAsync();

        var score = await service.CalculateHealthScoreAsync(oppHealthy);
        Assert.InRange(score, 0, 100);
        Assert.True(score >= 70, $"Expected healthy score >= 70, got {score}");
        Assert.Equal("Healthy", service.GetHealthLabel(score));
        Assert.Equal("success", service.GetHealthColor(score));
    }
}
