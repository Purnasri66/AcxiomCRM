using AcxiomCRM.Models.Entities;
using AcxiomCRM.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services;

public class FollowUpService : IFollowUpService
{
    private readonly IFollowUpRepository _followUpRepo;
    private readonly IAuditService _auditService;

    public FollowUpService(IFollowUpRepository followUpRepo, IAuditService auditService)
    {
        _followUpRepo = followUpRepo;
        _auditService = auditService;
    }

    public async Task<FollowUp?> GetByIdAsync(int id, string userId, string role)
    {
        var followUp = await _followUpRepo.GetByIdAsync(id);
        if (followUp == null) return null;
        if (role == "SalesExecutive" && followUp.AssignedTo != userId) return null;
        return followUp;
    }

    public async Task<FollowUp?> GetByIdWithDetailsAsync(int id, string userId, string role)
    {
        var followUp = await _followUpRepo.GetByIdWithDetailsAsync(id);
        if (followUp == null) return null;
        if (role == "SalesExecutive" && followUp.AssignedTo != userId) return null;
        return followUp;
    }

    public async Task<(IEnumerable<FollowUp> Items, int TotalCount)> GetPagedAsync(
        string userId, string role, string? statusFilter = null,
        int page = 1, int pageSize = 10, string? sortBy = null, bool ascending = true)
    {
        var scopedUserId = role == "SalesExecutive" ? userId : null;
        var query = _followUpRepo.GetFollowUpsQuery(scopedUserId);

        if (!string.IsNullOrWhiteSpace(statusFilter))
            query = query.Where(f => f.Status == statusFilter);

        var totalCount = await query.CountAsync();

        query = sortBy?.ToLower() switch
        {
            "date" => ascending ? query.OrderBy(f => f.FollowUpDate) : query.OrderByDescending(f => f.FollowUpDate),
            "status" => ascending ? query.OrderBy(f => f.Status) : query.OrderByDescending(f => f.Status),
            "type" => ascending ? query.OrderBy(f => f.FollowUpType) : query.OrderByDescending(f => f.FollowUpType),
            _ => query.OrderByDescending(f => f.FollowUpDate)
        };

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, totalCount);
    }

    public async Task<(bool Success, string? Error, FollowUp? FollowUp)> CreateAsync(FollowUp followUp, string userId, string role)
    {
        if (followUp.CustomerId == null && followUp.LeadId == null && followUp.OpportunityId == null)
            return (false, "At least one related record (Customer, Lead or Opportunity) must be selected.", null);

        if (followUp.FollowUpDate.Date < DateTime.Today)
            return (false, "Follow-up date cannot be earlier than today.", null);

        followUp.AssignedTo = role == "SalesExecutive" ? userId : followUp.AssignedTo;
        followUp.Status = "Planned";
        followUp.CreatedDate = DateTime.UtcNow;

        await _followUpRepo.AddAsync(followUp);
        await _followUpRepo.SaveChangesAsync();

        await _auditService.LogAsync(userId, "Create", "FollowUp", followUp.FollowUpId.ToString(), newValue: new
        {
            followUp.FollowUpType, followUp.FollowUpDate, followUp.Status, followUp.CustomerId, followUp.LeadId, followUp.OpportunityId
        });

        return (true, null, followUp);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(FollowUp followUp, string userId, string role)
    {
        var existing = await _followUpRepo.GetByIdAsync(followUp.FollowUpId);
        if (existing == null) return (false, "Follow-up not found.");
        if (role == "SalesExecutive" && existing.AssignedTo != userId) return (false, "Access denied.");

        if (followUp.CustomerId == null && followUp.LeadId == null && followUp.OpportunityId == null)
            return (false, "At least one related record must be selected.");

        var oldValue = new { existing.FollowUpType, existing.FollowUpDate, existing.Status };

        existing.FollowUpType = followUp.FollowUpType;
        existing.FollowUpDate = followUp.FollowUpDate;
        existing.Remarks = followUp.Remarks;
        existing.CustomerId = followUp.CustomerId;
        existing.LeadId = followUp.LeadId;
        existing.OpportunityId = followUp.OpportunityId;
        existing.ModifiedDate = DateTime.UtcNow;

        if (role != "SalesExecutive" && !string.IsNullOrEmpty(followUp.AssignedTo))
            existing.AssignedTo = followUp.AssignedTo;

        _followUpRepo.Update(existing);
        await _followUpRepo.SaveChangesAsync();

        await _auditService.LogAsync(userId, "Update", "FollowUp", followUp.FollowUpId.ToString(),
            oldValue: oldValue,
            newValue: new { existing.FollowUpType, existing.FollowUpDate, existing.Status });

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> CompleteAsync(int id, string? remarks, string userId, string role)
    {
        var followUp = await _followUpRepo.GetByIdAsync(id);
        if (followUp == null) return (false, "Follow-up not found.");
        if (role == "SalesExecutive" && followUp.AssignedTo != userId) return (false, "Access denied.");

        followUp.Status = "Completed";
        followUp.Remarks = remarks ?? followUp.Remarks;
        followUp.ModifiedDate = DateTime.UtcNow;

        _followUpRepo.Update(followUp);
        await _followUpRepo.SaveChangesAsync();

        await _auditService.LogAsync(userId, "Complete", "FollowUp", id.ToString(),
            oldValue: new { Status = "Planned" },
            newValue: new { Status = "Completed" });

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> RescheduleAsync(int id, DateTime newDate, string userId, string role)
    {
        if (newDate.Date < DateTime.Today)
            return (false, "Follow-up date cannot be earlier than today.");

        var followUp = await _followUpRepo.GetByIdAsync(id);
        if (followUp == null) return (false, "Follow-up not found.");
        if (role == "SalesExecutive" && followUp.AssignedTo != userId) return (false, "Access denied.");

        var oldDate = followUp.FollowUpDate;
        followUp.FollowUpDate = newDate;
        followUp.Status = "Planned";
        followUp.ModifiedDate = DateTime.UtcNow;

        _followUpRepo.Update(followUp);
        await _followUpRepo.SaveChangesAsync();

        await _auditService.LogAsync(userId, "Reschedule", "FollowUp", id.ToString(),
            oldValue: new { FollowUpDate = oldDate },
            newValue: new { FollowUpDate = newDate });

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> CancelAsync(int id, string userId, string role)
    {
        var followUp = await _followUpRepo.GetByIdAsync(id);
        if (followUp == null) return (false, "Follow-up not found.");
        if (role == "SalesExecutive" && followUp.AssignedTo != userId) return (false, "Access denied.");

        followUp.Status = "Cancelled";
        followUp.ModifiedDate = DateTime.UtcNow;

        _followUpRepo.Update(followUp);
        await _followUpRepo.SaveChangesAsync();

        await _auditService.LogAsync(userId, "Cancel", "FollowUp", id.ToString());
        return (true, null);
    }

    public async Task<IEnumerable<FollowUp>> GetOverdueAsync(string userId, string role)
    {
        var scopedUserId = role == "SalesExecutive" ? userId : null;
        return await _followUpRepo.GetOverdueFollowUpsAsync(scopedUserId);
    }

    public async Task<IEnumerable<FollowUp>> GetUpcomingAsync(string userId, string role, int days = 7)
    {
        var scopedUserId = role == "SalesExecutive" ? userId : null;
        return await _followUpRepo.GetUpcomingFollowUpsAsync(scopedUserId, days);
    }

    public async Task<int> GetOverdueCountAsync(string userId, string role)
    {
        var overdue = await GetOverdueAsync(userId, role);
        return overdue.Count();
    }
}
