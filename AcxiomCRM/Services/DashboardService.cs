using AcxiomCRM.Models.Entities;
using AcxiomCRM.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services;

public class DashboardService : IDashboardService
{
    private readonly ICustomerRepository _customerRepo;
    private readonly ILeadRepository _leadRepo;
    private readonly IOpportunityRepository _opportunityRepo;
    private readonly IFollowUpRepository _followUpRepo;
    private readonly IAuditLogRepository _auditLogRepo;
    private readonly IOpportunityService _opportunityService;
    private readonly IDealHealthService _healthService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardService(
        ICustomerRepository customerRepo,
        ILeadRepository leadRepo,
        IOpportunityRepository opportunityRepo,
        IFollowUpRepository followUpRepo,
        IAuditLogRepository auditLogRepo,
        IOpportunityService opportunityService,
        IDealHealthService healthService,
        UserManager<ApplicationUser> userManager)
    {
        _customerRepo = customerRepo;
        _leadRepo = leadRepo;
        _opportunityRepo = opportunityRepo;
        _followUpRepo = followUpRepo;
        _auditLogRepo = auditLogRepo;
        _opportunityService = opportunityService;
        _healthService = healthService;
        _userManager = userManager;
    }

    public async Task<DashboardData> GetDashboardDataAsync(string userId, string role, DateTime? startDate = null, DateTime? endDate = null)
    {
        var scopedUserId = role == "SalesExecutive" ? userId : null;
        var data = new DashboardData();

        var customerQuery = _customerRepo.GetCustomersQuery(scopedUserId);
        var leadQuery = _leadRepo.GetLeadsQuery(scopedUserId);
        var oppQuery = _opportunityRepo.GetOpportunitiesQuery(scopedUserId);
        var followUpQuery = _followUpRepo.GetFollowUpsQuery(scopedUserId);

        if (startDate.HasValue)
        {
            customerQuery = customerQuery.Where(c => c.CreatedDate >= startDate.Value);
            leadQuery = leadQuery.Where(l => l.CreatedDate >= startDate.Value);
            oppQuery = oppQuery.Where(o => o.CreatedDate >= startDate.Value);
        }
        if (endDate.HasValue)
        {
            customerQuery = customerQuery.Where(c => c.CreatedDate <= endDate.Value);
            leadQuery = leadQuery.Where(l => l.CreatedDate <= endDate.Value);
            oppQuery = oppQuery.Where(o => o.CreatedDate <= endDate.Value);
        }

        // Cards
        data.TotalCustomers = await customerQuery.CountAsync();
        data.TotalLeads = await leadQuery.CountAsync();
        data.OpenLeads = await leadQuery.Where(l => l.Status != "Lost" && l.Status != "Converted" && l.Status != "Unqualified").CountAsync();
        data.TotalOpportunities = await oppQuery.CountAsync();
        data.OpenOpportunities = await oppQuery.Where(o => o.Status == "Open").CountAsync();
        data.WonOpportunities = await oppQuery.Where(o => o.Status == "Won").CountAsync();
        data.LostOpportunities = await oppQuery.Where(o => o.Status == "Lost").CountAsync();
        data.TotalPipelineValue = await oppQuery.Where(o => o.Status == "Open").SumAsync(o => o.Amount);
        data.WeightedPipeline = await oppQuery.Where(o => o.Status == "Open").SumAsync(o => o.Amount * o.Probability / 100);

        // Follow-up stats
        data.UpcomingFollowUps = await followUpQuery.Where(f => f.Status == "Planned" && f.FollowUpDate >= DateTime.UtcNow && f.FollowUpDate <= DateTime.UtcNow.AddDays(7)).CountAsync();
        data.OverdueFollowUps = await followUpQuery.Where(f => f.Status == "Planned" && f.FollowUpDate < DateTime.UtcNow).CountAsync();

        // Lead status chart
        var leadStatuses = new[] { "New", "Contacted", "Qualified", "Lost", "Converted" };
        foreach (var status in leadStatuses)
        {
            data.LeadsByStatus[status] = await leadQuery.Where(l => l.Status == status).CountAsync();
        }

        // Opportunity stage chart
        var stages = new[] { "Qualification", "Proposal", "Negotiation", "Won", "Lost" };
        foreach (var stage in stages)
        {
            data.OpportunitiesByStage[stage] = await oppQuery.Where(o => o.Stage == stage).CountAsync();
        }

        // Monthly sales (last 6 months)
        for (int i = 5; i >= 0; i--)
        {
            var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-i);
            var monthEnd = monthStart.AddMonths(1);
            var monthLabel = monthStart.ToString("MMM yyyy");
            data.MonthlySales[monthLabel] = await oppQuery
                .Where(o => o.Status == "Won" && o.ExpectedCloseDate >= monthStart && o.ExpectedCloseDate < monthEnd)
                .SumAsync(o => o.Amount);
        }

        // Top deals at risk
        var atRiskDeals = await _opportunityService.GetAtRiskDealsAsync(userId, role, 5);
        data.TopDealsAtRisk = atRiskDeals.Select(o => new DealAtRisk
        {
            OpportunityId = o.OpportunityId,
            OpportunityName = o.OpportunityName,
            Amount = o.Amount,
            Stage = o.Stage,
            HealthScore = o.DealHealthScore,
            HealthColor = _healthService.GetHealthColor(o.DealHealthScore),
            HealthLabel = _healthService.GetHealthLabel(o.DealHealthScore)
        }).ToList();

        // Admin security stats
        if (role == "Admin")
        {
            var users = _userManager.Users;
            data.TotalUsers = await users.CountAsync();
            data.ActiveUsers = await users.Where(u => u.IsActive).CountAsync();
            data.InactiveUsers = data.TotalUsers - data.ActiveUsers;
            data.LockedAccounts = await users.Where(u => u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow).CountAsync();
            data.FailedLoginsLast7Days = await _auditLogRepo.Query()
                .Where(a => a.Action == "FailedLogin" && a.CreatedDate >= DateTime.UtcNow.AddDays(-7)).CountAsync();
            data.RecentAuditEvents = await _auditLogRepo.Query()
                .Where(a => a.CreatedDate >= DateTime.UtcNow.AddDays(-1)).CountAsync();
        }

        return data;
    }
}
