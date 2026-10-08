using AcxiomCRM.Models.Entities;
using AcxiomCRM.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services;

public class DealHealthService : IDealHealthService
{
    private readonly IFollowUpRepository _followUpRepo;
    private readonly IActivityRepository _activityRepo;

    public DealHealthService(IFollowUpRepository followUpRepo, IActivityRepository activityRepo)
    {
        _followUpRepo = followUpRepo;
        _activityRepo = activityRepo;
    }

    public async Task<int> CalculateHealthScoreAsync(Opportunity opportunity)
    {
        double score = 0;

        // Probability factor (0-25)
        score += opportunity.Probability * 0.25;

        // Days to expected close factor (0-25)
        if (opportunity.ExpectedCloseDate.HasValue)
        {
            var daysToClose = (opportunity.ExpectedCloseDate.Value - DateTime.UtcNow).TotalDays;
            score += daysToClose switch
            {
                > 60 => 25,
                > 30 => 20,
                > 14 => 15,
                > 7 => 10,
                > 0 => 5,
                _ => 0 // Past due
            };
        }
        else
        {
            score += 10; // No close date = moderate
        }

        // Days since last activity factor (0-25)
        var lastActivity = await _activityRepo.Query()
            .Where(a => a.CustomerId == opportunity.CustomerId)
            .OrderByDescending(a => a.ActivityDate)
            .FirstOrDefaultAsync();

        if (lastActivity != null)
        {
            var daysSinceActivity = (DateTime.UtcNow - lastActivity.ActivityDate).TotalDays;
            score += daysSinceActivity switch
            {
                <= 3 => 25,
                <= 7 => 20,
                <= 14 => 15,
                <= 30 => 10,
                _ => 0
            };
        }

        // Overdue follow-ups factor (0-25)
        var overdueCount = await _followUpRepo.Query()
            .Where(f => f.OpportunityId == opportunity.OpportunityId &&
                        f.Status == "Planned" && f.FollowUpDate < DateTime.UtcNow)
            .CountAsync();

        score += overdueCount switch
        {
            0 => 25,
            1 => 15,
            2 => 5,
            _ => 0
        };

        return Math.Min((int)Math.Round(score), 100);
    }

    public string GetHealthLabel(int score) => score switch
    {
        >= 70 => "Healthy",
        >= 40 => "At Risk",
        _ => "Critical"
    };

    public string GetHealthColor(int score) => score switch
    {
        >= 70 => "success",
        >= 40 => "warning",
        _ => "danger"
    };
}
