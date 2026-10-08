using AcxiomCRM.Models.Entities;
using AcxiomCRM.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services;

public class ActivityService : IActivityService
{
    private readonly IActivityRepository _activityRepo;
    private readonly IAuditService _auditService;
    private static readonly HashSet<string> ValidTypes = new() { "Call", "Meeting", "Email", "Task" };

    public ActivityService(IActivityRepository activityRepo, IAuditService auditService)
    {
        _activityRepo = activityRepo;
        _auditService = auditService;
    }

    public async Task<Activity?> GetByIdAsync(int id, string userId, string role)
    {
        var activity = await _activityRepo.GetByIdAsync(id);
        if (activity == null) return null;
        if (role == "SalesExecutive" && activity.AssignedTo != userId) return null;
        return activity;
    }

    public async Task<Activity?> GetByIdWithDetailsAsync(int id, string userId, string role)
    {
        var activity = await _activityRepo.GetByIdWithDetailsAsync(id);
        if (activity == null) return null;
        if (role == "SalesExecutive" && activity.AssignedTo != userId) return null;
        return activity;
    }

    public async Task<(IEnumerable<Activity> Items, int TotalCount)> GetPagedAsync(
        string userId, string role, string? typeFilter = null,
        int page = 1, int pageSize = 10, string? sortBy = null, bool ascending = true)
    {
        var scopedUserId = role == "SalesExecutive" ? userId : null;
        var query = _activityRepo.GetActivitiesQuery(scopedUserId);

        if (!string.IsNullOrWhiteSpace(typeFilter))
            query = query.Where(a => a.ActivityType == typeFilter);

        var totalCount = await query.CountAsync();

        query = sortBy?.ToLower() switch
        {
            "type" => ascending ? query.OrderBy(a => a.ActivityType) : query.OrderByDescending(a => a.ActivityType),
            "subject" => ascending ? query.OrderBy(a => a.Subject) : query.OrderByDescending(a => a.Subject),
            "date" => ascending ? query.OrderBy(a => a.ActivityDate) : query.OrderByDescending(a => a.ActivityDate),
            "status" => ascending ? query.OrderBy(a => a.Status) : query.OrderByDescending(a => a.Status),
            _ => query.OrderByDescending(a => a.ActivityDate)
        };

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, totalCount);
    }

    public async Task<(bool Success, string? Error, Activity? Activity)> CreateAsync(Activity activity, string userId, string role)
    {
        if (!ValidTypes.Contains(activity.ActivityType))
            return (false, $"'{activity.ActivityType}' is not a valid activity type. Valid types: Call, Meeting, Email, Task.", null);

        activity.AssignedTo = role == "SalesExecutive" ? userId : activity.AssignedTo;
        activity.CreatedDate = DateTime.UtcNow;

        await _activityRepo.AddAsync(activity);
        await _activityRepo.SaveChangesAsync();

        await _auditService.LogAsync(userId, "Create", "Activity", activity.ActivityId.ToString(), newValue: new
        {
            activity.ActivityType, activity.Subject, activity.ActivityDate
        });

        return (true, null, activity);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(Activity activity, string userId, string role)
    {
        var existing = await _activityRepo.GetByIdAsync(activity.ActivityId);
        if (existing == null) return (false, "Activity not found.");
        if (role == "SalesExecutive" && existing.AssignedTo != userId) return (false, "Access denied.");

        if (!ValidTypes.Contains(activity.ActivityType))
            return (false, $"'{activity.ActivityType}' is not a valid activity type.");

        var oldValue = new { existing.ActivityType, existing.Subject, existing.Status };

        existing.ActivityType = activity.ActivityType;
        existing.Subject = activity.Subject;
        existing.Description = activity.Description;
        existing.ActivityDate = activity.ActivityDate;
        existing.Status = activity.Status;
        existing.CustomerId = activity.CustomerId;
        existing.LeadId = activity.LeadId;
        existing.ModifiedDate = DateTime.UtcNow;

        if (role != "SalesExecutive" && !string.IsNullOrEmpty(activity.AssignedTo))
            existing.AssignedTo = activity.AssignedTo;

        _activityRepo.Update(existing);
        await _activityRepo.SaveChangesAsync();

        await _auditService.LogAsync(userId, "Update", "Activity", activity.ActivityId.ToString(),
            oldValue: oldValue,
            newValue: new { existing.ActivityType, existing.Subject, existing.Status });

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id, string userId, string role)
    {
        var existing = await _activityRepo.GetByIdAsync(id);
        if (existing == null) return (false, "Activity not found.");
        if (role == "SalesExecutive" && existing.AssignedTo != userId) return (false, "Access denied.");

        existing.IsActive = false;
        existing.ModifiedDate = DateTime.UtcNow;
        _activityRepo.Update(existing);
        await _activityRepo.SaveChangesAsync();

        await _auditService.LogAsync(userId, "Delete", "Activity", id.ToString());
        return (true, null);
    }
}
