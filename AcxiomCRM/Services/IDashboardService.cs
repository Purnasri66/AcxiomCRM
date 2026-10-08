namespace AcxiomCRM.Services;

/// <summary>
/// Service providing dashboard statistics and chart data.
/// </summary>
public interface IDashboardService
{
    Task<DashboardData> GetDashboardDataAsync(string userId, string role, DateTime? startDate = null, DateTime? endDate = null);
}

public class DashboardData
{
    public int TotalCustomers { get; set; }
    public int TotalLeads { get; set; }
    public int OpenLeads { get; set; }
    public int TotalOpportunities { get; set; }
    public int OpenOpportunities { get; set; }
    public int WonOpportunities { get; set; }
    public int LostOpportunities { get; set; }
    public decimal TotalPipelineValue { get; set; }
    public decimal WeightedPipeline { get; set; }

    // Admin security stats
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public int LockedAccounts { get; set; }
    public int FailedLoginsLast7Days { get; set; }
    public int RecentAuditEvents { get; set; }

    // Chart data
    public Dictionary<string, int> LeadsByStatus { get; set; } = new();
    public Dictionary<string, int> OpportunitiesByStage { get; set; } = new();
    public Dictionary<string, decimal> MonthlySales { get; set; } = new();
    public List<DealAtRisk> TopDealsAtRisk { get; set; } = new();
    public int UpcomingFollowUps { get; set; }
    public int OverdueFollowUps { get; set; }
}

public class DealAtRisk
{
    public int OpportunityId { get; set; }
    public string OpportunityName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Stage { get; set; } = string.Empty;
    public int HealthScore { get; set; }
    public string HealthColor { get; set; } = string.Empty;
    public string HealthLabel { get; set; } = string.Empty;
}
