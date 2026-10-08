using AcxiomCRM.Models.Entities;
using AcxiomCRM.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services;

public class LeadService : ILeadService
{
    private readonly ILeadRepository _leadRepo;
    private readonly ICustomerRepository _customerRepo;
    private readonly IOpportunityRepository _opportunityRepo;
    private readonly IAuditService _auditService;
    private readonly ILeadScoringService _scoringService;

    // Lead status transition map
    private static readonly Dictionary<string, List<string>> TransitionMap = new()
    {
        { "New", new List<string> { "Contacted", "Lost" } },
        { "Contacted", new List<string> { "Qualified", "Lost" } },
        { "Qualified", new List<string> { "Converted", "Unqualified", "Lost" } },
        { "Converted", new List<string>() },
        { "Lost", new List<string>() },
        { "Unqualified", new List<string> { "Lost" } }
    };

    public LeadService(ILeadRepository leadRepo, ICustomerRepository customerRepo,
                       IOpportunityRepository opportunityRepo, IAuditService auditService,
                       ILeadScoringService scoringService)
    {
        _leadRepo = leadRepo;
        _customerRepo = customerRepo;
        _opportunityRepo = opportunityRepo;
        _auditService = auditService;
        _scoringService = scoringService;
    }

    public async Task<Lead?> GetByIdAsync(int id, string userId, string role)
    {
        var lead = await _leadRepo.GetByIdAsync(id);
        if (lead == null) return null;
        if (role == "SalesExecutive" && lead.AssignedTo != userId) return null;
        return lead;
    }

    public async Task<Lead?> GetByIdWithDetailsAsync(int id, string userId, string role)
    {
        var lead = await _leadRepo.GetByIdWithDetailsAsync(id);
        if (lead == null) return null;
        if (role == "SalesExecutive" && lead.AssignedTo != userId) return null;
        return lead;
    }

    public async Task<(IEnumerable<Lead> Items, int TotalCount)> GetPagedAsync(
        string userId, string role, string? search = null, string? statusFilter = null,
        int page = 1, int pageSize = 10, string? sortBy = null, bool ascending = true)
    {
        var scopedUserId = role == "SalesExecutive" ? userId : null;
        var query = _leadRepo.GetLeadsQuery(scopedUserId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(l =>
                l.LeadName.ToLower().Contains(s) ||
                l.Email.ToLower().Contains(s) ||
                (l.CompanyName != null && l.CompanyName.ToLower().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter))
            query = query.Where(l => l.Status == statusFilter);

        var totalCount = await query.CountAsync();

        query = sortBy?.ToLower() switch
        {
            "name" => ascending ? query.OrderBy(l => l.LeadName) : query.OrderByDescending(l => l.LeadName),
            "status" => ascending ? query.OrderBy(l => l.Status) : query.OrderByDescending(l => l.Status),
            "value" => ascending ? query.OrderBy(l => l.ExpectedValue) : query.OrderByDescending(l => l.ExpectedValue),
            "score" => ascending ? query.OrderBy(l => l.LeadScore) : query.OrderByDescending(l => l.LeadScore),
            "date" => ascending ? query.OrderBy(l => l.CreatedDate) : query.OrderByDescending(l => l.CreatedDate),
            _ => query.OrderByDescending(l => l.LeadScore).ThenByDescending(l => l.CreatedDate)
        };

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, totalCount);
    }

    public async Task<(bool Success, string? Error, Lead? Lead)> CreateAsync(Lead lead, string userId, string role)
    {
        lead.LeadCode = await _leadRepo.GenerateLeadCodeAsync();
        lead.AssignedTo = role == "SalesExecutive" ? userId : lead.AssignedTo;
        lead.CreatedDate = DateTime.UtcNow;
        lead.Status = "New";
        _scoringService.CalculateScore(lead);

        await _leadRepo.AddAsync(lead);
        await _leadRepo.SaveChangesAsync();

        await _auditService.LogAsync(userId, "Create", "Lead", lead.LeadId.ToString(), newValue: new
        {
            lead.LeadCode, lead.LeadName, lead.Email, lead.Source, lead.ExpectedValue, lead.Status
        });

        return (true, null, lead);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(Lead lead, string userId, string role)
    {
        var existing = await _leadRepo.GetByIdAsync(lead.LeadId);
        if (existing == null) return (false, "Lead not found.");
        if (role == "SalesExecutive" && existing.AssignedTo != userId) return (false, "Access denied.");

        var oldValue = new { existing.LeadName, existing.Email, existing.Status, existing.ExpectedValue };

        existing.LeadName = lead.LeadName;
        existing.Email = lead.Email;
        existing.Phone = lead.Phone;
        existing.CompanyName = lead.CompanyName;
        existing.Source = lead.Source;
        existing.ExpectedValue = lead.ExpectedValue;
        existing.ModifiedDate = DateTime.UtcNow;

        if (role != "SalesExecutive" && !string.IsNullOrEmpty(lead.AssignedTo))
            existing.AssignedTo = lead.AssignedTo;

        _scoringService.CalculateScore(existing);
        _leadRepo.Update(existing);
        await _leadRepo.SaveChangesAsync();

        await _auditService.LogAsync(userId, "Update", "Lead", lead.LeadId.ToString(),
            oldValue: oldValue,
            newValue: new { existing.LeadName, existing.Email, existing.Status, existing.ExpectedValue });

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id, string userId, string role)
    {
        var existing = await _leadRepo.GetByIdAsync(id);
        if (existing == null) return (false, "Lead not found.");
        if (role == "SalesExecutive" && existing.AssignedTo != userId) return (false, "Access denied.");

        existing.IsDeleted = true;
        existing.IsActive = false;
        existing.ModifiedDate = DateTime.UtcNow;
        _leadRepo.Update(existing);
        await _leadRepo.SaveChangesAsync();

        await _auditService.LogAsync(userId, "Delete", "Lead", id.ToString());
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ChangeStatusAsync(int id, string newStatus, string userId, string role)
    {
        var lead = await _leadRepo.GetByIdAsync(id);
        if (lead == null) return (false, "Lead not found.");
        if (role == "SalesExecutive" && lead.AssignedTo != userId) return (false, "Access denied.");

        if (!TransitionMap.TryGetValue(lead.Status, out var validTransitions) || !validTransitions.Contains(newStatus))
            return (false, $"Cannot transition from '{lead.Status}' to '{newStatus}'.");

        var oldStatus = lead.Status;
        lead.Status = newStatus;
        lead.ModifiedDate = DateTime.UtcNow;
        _scoringService.CalculateScore(lead);

        if (newStatus == "Lost" || newStatus == "Unqualified")
            lead.IsActive = false;

        _leadRepo.Update(lead);
        await _leadRepo.SaveChangesAsync();

        await _auditService.LogAsync(userId, "StatusChange", "Lead", id.ToString(),
            oldValue: new { Status = oldStatus },
            newValue: new { Status = newStatus });

        return (true, null);
    }

    public async Task<(bool Success, string? Error, int? CustomerId)> ConvertAsync(int leadId, string? opportunityName, decimal? amount, string userId, string role)
    {
        var lead = await _leadRepo.GetByIdWithDetailsAsync(leadId);
        if (lead == null) return (false, "Lead not found.", null);
        if (role == "SalesExecutive" && lead.AssignedTo != userId) return (false, "Access denied.", null);
        if (lead.Status != "Qualified") return (false, "Only Qualified leads can be converted.", null);

        // Duplicate check by email
        var existingCustomer = await _customerRepo.GetByEmailAsync(lead.Email);
        if (existingCustomer != null)
            return (false, $"A customer with email '{lead.Email}' already exists (Code: {existingCustomer.CustomerCode}).", null);

        var customer = new Customer
        {
            CustomerCode = await _customerRepo.GenerateCustomerCodeAsync(),
            CustomerName = lead.LeadName,
            Email = lead.Email,
            Phone = lead.Phone,
            CompanyName = lead.CompanyName,
            Status = "Active",
            CreatedBy = userId,
            CreatedDate = DateTime.UtcNow
        };

        await _customerRepo.AddAsync(customer);
        await _customerRepo.SaveChangesAsync();

        lead.Status = "Converted";
        lead.ConvertedCustomerId = customer.CustomerId;
        lead.ModifiedDate = DateTime.UtcNow;
        _leadRepo.Update(lead);

        // Create opportunity if requested
        Opportunity? opportunity = null;
        if (!string.IsNullOrWhiteSpace(opportunityName) && amount.HasValue && amount.Value > 0)
        {
            opportunity = new Opportunity
            {
                OpportunityName = opportunityName,
                CustomerId = customer.CustomerId,
                LeadId = lead.LeadId,
                Amount = amount.Value,
                Stage = "Qualification",
                Probability = 20,
                ExpectedCloseDate = DateTime.UtcNow.AddDays(90),
                Status = "Open",
                AssignedTo = lead.AssignedTo,
                CreatedDate = DateTime.UtcNow
            };

            await _opportunityRepo.AddAsync(opportunity);
        }

        await _leadRepo.SaveChangesAsync();

        await _auditService.LogAsync(userId, "Convert", "Lead", leadId.ToString(),
            oldValue: new { lead.LeadCode, Status = "Qualified" },
            newValue: new { Status = "Converted", CustomerId = customer.CustomerId, CustomerCode = customer.CustomerCode, OpportunityCreated = opportunity != null });

        return (true, null, customer.CustomerId);
    }

    public IEnumerable<string> GetValidTransitions(string currentStatus)
    {
        return TransitionMap.TryGetValue(currentStatus, out var transitions) ? transitions : Enumerable.Empty<string>();
    }
}
