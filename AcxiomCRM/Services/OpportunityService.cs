using AcxiomCRM.Models.Entities;
using AcxiomCRM.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services;

public class OpportunityService : IOpportunityService
{
    private readonly IOpportunityRepository _opportunityRepo;
    private readonly IAuditService _auditService;
    private readonly IDealHealthService _healthService;

    private static readonly string[] ValidStages = { "Qualification", "Proposal", "Negotiation", "Won", "Lost" };

    public OpportunityService(IOpportunityRepository opportunityRepo, IAuditService auditService, IDealHealthService healthService)
    {
        _opportunityRepo = opportunityRepo;
        _auditService = auditService;
        _healthService = healthService;
    }

    public async Task<Opportunity?> GetByIdAsync(int id, string userId, string role)
    {
        var opp = await _opportunityRepo.GetByIdAsync(id);
        if (opp == null) return null;
        if (role == "SalesExecutive" && opp.AssignedTo != userId) return null;
        return opp;
    }

    public async Task<Opportunity?> GetByIdWithDetailsAsync(int id, string userId, string role)
    {
        var opp = await _opportunityRepo.GetByIdWithDetailsAsync(id);
        if (opp == null) return null;
        if (role == "SalesExecutive" && opp.AssignedTo != userId) return null;
        opp.DealHealthScore = await _healthService.CalculateHealthScoreAsync(opp);
        return opp;
    }

    public async Task<(IEnumerable<Opportunity> Items, int TotalCount)> GetPagedAsync(
        string userId, string role, string? search = null, string? stageFilter = null,
        int page = 1, int pageSize = 10, string? sortBy = null, bool ascending = true)
    {
        var scopedUserId = role == "SalesExecutive" ? userId : null;
        var query = _opportunityRepo.GetOpportunitiesQuery(scopedUserId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(o =>
                o.OpportunityName.ToLower().Contains(s) ||
                (o.Customer != null && o.Customer.CustomerName.ToLower().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(stageFilter))
            query = query.Where(o => o.Stage == stageFilter);

        var totalCount = await query.CountAsync();

        query = sortBy?.ToLower() switch
        {
            "name" => ascending ? query.OrderBy(o => o.OpportunityName) : query.OrderByDescending(o => o.OpportunityName),
            "amount" => ascending ? query.OrderBy(o => o.Amount) : query.OrderByDescending(o => o.Amount),
            "stage" => ascending ? query.OrderBy(o => o.Stage) : query.OrderByDescending(o => o.Stage),
            "date" => ascending ? query.OrderBy(o => o.CreatedDate) : query.OrderByDescending(o => o.CreatedDate),
            "close" => ascending ? query.OrderBy(o => o.ExpectedCloseDate) : query.OrderByDescending(o => o.ExpectedCloseDate),
            _ => query.OrderByDescending(o => o.CreatedDate)
        };

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, totalCount);
    }

    public async Task<(bool Success, string? Error, Opportunity? Opportunity)> CreateAsync(Opportunity opportunity, string userId, string role)
    {
        if (opportunity.Amount <= 0 && opportunity.Status != "Lost")
            return (false, "Opportunity Amount must be greater than 0.", null);

        if (opportunity.Probability < 0 || opportunity.Probability > 100)
            return (false, "Probability must be between 0 and 100.", null);

        if (opportunity.ExpectedCloseDate.HasValue && opportunity.ExpectedCloseDate.Value.Date < DateTime.Today && opportunity.Status == "Open")
            return (false, "Expected Close Date cannot be in the past.", null);

        opportunity.AssignedTo = role == "SalesExecutive" ? userId : opportunity.AssignedTo;
        opportunity.CreatedDate = DateTime.UtcNow;

        await _opportunityRepo.AddAsync(opportunity);
        await _opportunityRepo.SaveChangesAsync();

        await _auditService.LogAsync(userId, "Create", "Opportunity", opportunity.OpportunityId.ToString(), newValue: new
        {
            opportunity.OpportunityName, opportunity.Amount, opportunity.Stage, opportunity.Probability
        });

        return (true, null, opportunity);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(Opportunity opportunity, string userId, string role)
    {
        var existing = await _opportunityRepo.GetByIdAsync(opportunity.OpportunityId);
        if (existing == null) return (false, "Opportunity not found.");
        if (role == "SalesExecutive" && existing.AssignedTo != userId) return (false, "Access denied.");

        if (opportunity.Amount <= 0 && opportunity.Status != "Lost")
            return (false, "Opportunity Amount must be greater than 0.");

        if (opportunity.Probability < 0 || opportunity.Probability > 100)
            return (false, "Probability must be between 0 and 100.");

        var oldValue = new { existing.OpportunityName, existing.Amount, existing.Stage, existing.Probability, existing.Status };

        existing.OpportunityName = opportunity.OpportunityName;
        existing.Amount = opportunity.Amount;
        existing.Stage = opportunity.Stage;
        existing.Probability = opportunity.Probability;
        existing.ExpectedCloseDate = opportunity.ExpectedCloseDate;
        existing.Status = opportunity.Status;
        existing.CustomerId = opportunity.CustomerId;
        existing.ModifiedDate = DateTime.UtcNow;

        if (role != "SalesExecutive" && !string.IsNullOrEmpty(opportunity.AssignedTo))
            existing.AssignedTo = opportunity.AssignedTo;

        _opportunityRepo.Update(existing);
        await _opportunityRepo.SaveChangesAsync();

        await _auditService.LogAsync(userId, "Update", "Opportunity", opportunity.OpportunityId.ToString(),
            oldValue: oldValue,
            newValue: new { existing.OpportunityName, existing.Amount, existing.Stage, existing.Probability, existing.Status });

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id, string userId, string role)
    {
        var existing = await _opportunityRepo.GetByIdAsync(id);
        if (existing == null) return (false, "Opportunity not found.");
        if (role == "SalesExecutive" && existing.AssignedTo != userId) return (false, "Access denied.");

        existing.IsDeleted = true;
        existing.IsActive = false;
        existing.ModifiedDate = DateTime.UtcNow;
        _opportunityRepo.Update(existing);
        await _opportunityRepo.SaveChangesAsync();

        await _auditService.LogAsync(userId, "Delete", "Opportunity", id.ToString());
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ChangeStageAsync(int id, string newStage, string userId, string role)
    {
        if (!ValidStages.Contains(newStage))
            return (false, $"'{newStage}' is not a valid pipeline stage.");

        var opp = await _opportunityRepo.GetByIdAsync(id);
        if (opp == null) return (false, "Opportunity not found.");
        if (role == "SalesExecutive" && opp.AssignedTo != userId) return (false, "Access denied.");

        var oldStage = opp.Stage;
        opp.Stage = newStage;
        opp.ModifiedDate = DateTime.UtcNow;

        if (newStage == "Won") { opp.Status = "Won"; opp.Probability = 100; }
        else if (newStage == "Lost") { opp.Status = "Lost"; opp.Probability = 0; opp.IsActive = false; }
        else { opp.Status = "Open"; }

        _opportunityRepo.Update(opp);
        await _opportunityRepo.SaveChangesAsync();

        await _auditService.LogAsync(userId, "StageChange", "Opportunity", id.ToString(),
            oldValue: new { Stage = oldStage },
            newValue: new { Stage = newStage });

        return (true, null);
    }

    public async Task<Dictionary<string, List<Opportunity>>> GetKanbanDataAsync(string userId, string role)
    {
        var scopedUserId = role == "SalesExecutive" ? userId : null;
        var opps = await _opportunityRepo.GetOpportunitiesQuery(scopedUserId)
            .Where(o => o.IsActive || o.Stage == "Won" || o.Stage == "Lost")
            .ToListAsync();

        var kanban = new Dictionary<string, List<Opportunity>>();
        foreach (var stage in ValidStages)
        {
            kanban[stage] = opps.Where(o => o.Stage == stage).ToList();
        }

        // Calculate health scores
        foreach (var opp in opps.Where(o => o.Status == "Open"))
        {
            opp.DealHealthScore = await _healthService.CalculateHealthScoreAsync(opp);
        }

        return kanban;
    }

    public async Task<IEnumerable<Opportunity>> GetAtRiskDealsAsync(string userId, string role, int count = 5)
    {
        var scopedUserId = role == "SalesExecutive" ? userId : null;
        var openOpps = await _opportunityRepo.GetOpportunitiesQuery(scopedUserId)
            .Where(o => o.Status == "Open")
            .ToListAsync();

        foreach (var opp in openOpps)
        {
            opp.DealHealthScore = await _healthService.CalculateHealthScoreAsync(opp);
        }

        return openOpps.OrderBy(o => o.DealHealthScore).Take(count);
    }
}
