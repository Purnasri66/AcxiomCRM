using System.Security.Claims;
using AcxiomCRM.Models.ViewModels;
using AcxiomCRM.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace AcxiomCRM.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class AuditController : Controller
{
    private readonly IAuditLogRepository _auditRepo;

    public AuditController(IAuditLogRepository auditRepo)
    {
        _auditRepo = auditRepo;
    }

    public async Task<IActionResult> Index(string? entityFilter, string? actionFilter, DateTime? startDate, DateTime? endDate, int page = 1)
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "Manager";
        var query = _auditRepo.GetAuditLogsQuery();

        if (role == "Manager")
        {
            // Manager: limited view (no security/user admin events)
            query = query.Where(a => a.EntityName != "User" && a.EntityName != "Role" && a.Action != "FailedLogin" && a.Action != "Lockout");
        }

        if (!string.IsNullOrWhiteSpace(entityFilter))
            query = query.Where(a => a.EntityName == entityFilter);

        if (!string.IsNullOrWhiteSpace(actionFilter))
            query = query.Where(a => a.Action == actionFilter);

        if (startDate.HasValue)
            query = query.Where(a => a.CreatedDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.CreatedDate <= endDate.Value.AddDays(1));

        var pageSize = 20;
        var totalCount = await query.CountAsync();
        var logs = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        var items = new List<AuditLogItemViewModel>();
        foreach (var l in logs)
        {
            var diffs = ComputeDiff(l.OldValue, l.NewValue);
            items.Add(new AuditLogItemViewModel
            {
                AuditLogId = l.AuditLogId,
                UserId = l.UserId,
                Action = l.Action,
                EntityName = l.EntityName,
                RecordId = l.RecordId,
                OldValue = l.OldValue,
                NewValue = l.NewValue,
                CreatedDate = l.CreatedDate,
                IpAddress = l.IpAddress,
                DiffItems = diffs
            });
        }

        var model = new AuditLogListViewModel
        {
            Items = items,
            EntityFilter = entityFilter,
            ActionFilter = actionFilter,
            StartDate = startDate,
            EndDate = endDate,
            CurrentPage = page,
            TotalCount = totalCount,
            PageSize = pageSize
        };

        return View(model);
    }

    private static List<AuditDiffItemViewModel> ComputeDiff(string? oldJson, string? newJson)
    {
        var diffs = new List<AuditDiffItemViewModel>();
        if (string.IsNullOrEmpty(oldJson) && string.IsNullOrEmpty(newJson))
            return diffs;

        try
        {
            var oldObj = !string.IsNullOrEmpty(oldJson) ? JObject.Parse(oldJson) : new JObject();
            var newObj = !string.IsNullOrEmpty(newJson) ? JObject.Parse(newJson) : new JObject();

            var allKeys = oldObj.Properties().Select(p => p.Name)
                .Union(newObj.Properties().Select(p => p.Name))
                .Distinct();

            foreach (var key in allKeys)
            {
                var oldVal = oldObj[key]?.ToString();
                var newVal = newObj[key]?.ToString();

                if (oldVal != newVal)
                {
                    diffs.Add(new AuditDiffItemViewModel
                    {
                        FieldName = key,
                        OldValue = oldVal ?? "(none)",
                        NewValue = newVal ?? "(none)"
                    });
                }
            }
        }
        catch
        {
            // If parsing fails, show full strings
            diffs.Add(new AuditDiffItemViewModel
            {
                FieldName = "Raw Diff",
                OldValue = oldJson,
                NewValue = newJson
            });
        }

        return diffs;
    }
}
