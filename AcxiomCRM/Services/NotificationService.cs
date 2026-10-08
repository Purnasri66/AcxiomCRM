using AcxiomCRM.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services;

public class NotificationService : INotificationService
{
    private readonly IFollowUpRepository _followUpRepo;

    public NotificationService(IFollowUpRepository followUpRepo)
    {
        _followUpRepo = followUpRepo;
    }

    public async Task<NotificationData> GetNotificationsAsync(string userId, string role)
    {
        var scopedUserId = role == "SalesExecutive" ? userId : null;
        var data = new NotificationData();

        var overdueFollowUps = await _followUpRepo.GetOverdueFollowUpsAsync(scopedUserId);
        foreach (var f in overdueFollowUps.Take(10))
        {
            data.Items.Add(new NotificationItem
            {
                Title = "Overdue Follow-Up",
                Message = $"{f.FollowUpType} - {f.Customer?.CustomerName ?? f.Lead?.LeadName ?? "Opportunity"} (Due: {f.FollowUpDate:MMM dd})",
                Type = "overdue",
                Url = $"/FollowUps/Details/{f.FollowUpId}",
                Date = f.FollowUpDate
            });
        }

        var upcomingFollowUps = await _followUpRepo.GetUpcomingFollowUpsAsync(scopedUserId, 3);
        foreach (var f in upcomingFollowUps.Take(10))
        {
            data.Items.Add(new NotificationItem
            {
                Title = "Upcoming Follow-Up",
                Message = $"{f.FollowUpType} - {f.Customer?.CustomerName ?? f.Lead?.LeadName ?? "Opportunity"} (Due: {f.FollowUpDate:MMM dd})",
                Type = "upcoming",
                Url = $"/FollowUps/Details/{f.FollowUpId}",
                Date = f.FollowUpDate
            });
        }

        data.TotalCount = data.Items.Count;
        return data;
    }
}
