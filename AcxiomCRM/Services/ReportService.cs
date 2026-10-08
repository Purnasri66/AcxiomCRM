using AcxiomCRM.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services;

public class ReportService : IReportService
{
    private readonly ICustomerRepository _customerRepo;
    private readonly ILeadRepository _leadRepo;
    private readonly IOpportunityRepository _opportunityRepo;
    private readonly IFollowUpRepository _followUpRepo;
    private readonly IAuditLogRepository _auditLogRepo;

    public ReportService(ICustomerRepository customerRepo, ILeadRepository leadRepo,
        IOpportunityRepository opportunityRepo, IFollowUpRepository followUpRepo,
        IAuditLogRepository auditLogRepo)
    {
        _customerRepo = customerRepo;
        _leadRepo = leadRepo;
        _opportunityRepo = opportunityRepo;
        _followUpRepo = followUpRepo;
        _auditLogRepo = auditLogRepo;
    }

    public async Task<object> GetCustomerReportAsync(string userId, string role, string? search = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 25)
    {
        var scopedUserId = role == "SalesExecutive" ? userId : null;
        var query = _customerRepo.GetCustomersQuery(scopedUserId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(c => c.CustomerName.ToLower().Contains(s) || c.Email.ToLower().Contains(s));
        }
        if (startDate.HasValue) query = query.Where(c => c.CreatedDate >= startDate.Value);
        if (endDate.HasValue) query = query.Where(c => c.CreatedDate <= endDate.Value);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(c => c.CreatedDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<object> GetLeadReportAsync(string userId, string role, string? statusFilter = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 25)
    {
        var scopedUserId = role == "SalesExecutive" ? userId : null;
        var query = _leadRepo.GetLeadsQuery(scopedUserId);

        if (!string.IsNullOrWhiteSpace(statusFilter)) query = query.Where(l => l.Status == statusFilter);
        if (startDate.HasValue) query = query.Where(l => l.CreatedDate >= startDate.Value);
        if (endDate.HasValue) query = query.Where(l => l.CreatedDate <= endDate.Value);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(l => l.CreatedDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<object> GetOpportunityReportAsync(string userId, string role, string? stageFilter = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 25)
    {
        var scopedUserId = role == "SalesExecutive" ? userId : null;
        var query = _opportunityRepo.GetOpportunitiesQuery(scopedUserId);

        if (!string.IsNullOrWhiteSpace(stageFilter)) query = query.Where(o => o.Stage == stageFilter);
        if (startDate.HasValue) query = query.Where(o => o.CreatedDate >= startDate.Value);
        if (endDate.HasValue) query = query.Where(o => o.CreatedDate <= endDate.Value);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(o => o.CreatedDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<object> GetFollowUpReportAsync(string userId, string role, string? statusFilter = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 25)
    {
        var scopedUserId = role == "SalesExecutive" ? userId : null;
        var query = _followUpRepo.GetFollowUpsQuery(scopedUserId);

        if (!string.IsNullOrWhiteSpace(statusFilter)) query = query.Where(f => f.Status == statusFilter);
        if (startDate.HasValue) query = query.Where(f => f.FollowUpDate >= startDate.Value);
        if (endDate.HasValue) query = query.Where(f => f.FollowUpDate <= endDate.Value);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(f => f.FollowUpDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<object> GetPipelineReportAsync(string userId, string role)
    {
        var scopedUserId = role == "SalesExecutive" ? userId : null;
        var query = _opportunityRepo.GetOpportunitiesQuery(scopedUserId).Where(o => o.Status == "Open");

        var stages = new[] { "Qualification", "Proposal", "Negotiation" };
        var pipelineData = new List<object>();

        foreach (var stage in stages)
        {
            var stageOpps = await query.Where(o => o.Stage == stage).ToListAsync();
            pipelineData.Add(new
            {
                Stage = stage,
                Count = stageOpps.Count,
                TotalValue = stageOpps.Sum(o => o.Amount),
                WeightedValue = stageOpps.Sum(o => o.Amount * o.Probability / 100)
            });
        }

        return new { Pipeline = pipelineData };
    }

    public async Task<object> GetAuditReportAsync(string userId, string role, string? entityFilter = null, string? actionFilter = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 25)
    {
        if (role != "Admin" && role != "Manager")
            return new { Items = new List<object>(), TotalCount = 0, Page = page, PageSize = pageSize };

        var query = _auditLogRepo.GetAuditLogsQuery();

        if (!string.IsNullOrWhiteSpace(entityFilter)) query = query.Where(a => a.EntityName == entityFilter);
        if (!string.IsNullOrWhiteSpace(actionFilter)) query = query.Where(a => a.Action == actionFilter);
        if (startDate.HasValue) query = query.Where(a => a.CreatedDate >= startDate.Value);
        if (endDate.HasValue) query = query.Where(a => a.CreatedDate <= endDate.Value);

        // Manager: limited view (no security events)
        if (role == "Manager")
            query = query.Where(a => a.EntityName != "User" && a.EntityName != "Role" && a.Action != "FailedLogin" && a.Action != "Lockout");

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }
}
