using System.Security.Claims;
using AcxiomCRM.Models.Entities;
using AcxiomCRM.Models.ViewModels;
using AcxiomCRM.Repositories;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Controllers;

public class CustomerController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly IAuditLogRepository _auditRepo;

    public CustomerController(ICustomerService customerService, IAuditLogRepository auditRepo)
    {
        _customerService = customerService;
        _auditRepo = auditRepo;
    }

    public async Task<IActionResult> Index(string? search, int page = 1, string? sortBy = null, bool ascending = true)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var (items, totalCount) = await _customerService.GetPagedAsync(userId, role, search, page, 10, sortBy, ascending);

        var model = new CustomerListViewModel
        {
            Customers = items.Select(c => new CustomerItemViewModel
            {
                CustomerId = c.CustomerId,
                CustomerCode = c.CustomerCode,
                CustomerName = c.CustomerName,
                Email = c.Email,
                Phone = c.Phone,
                CompanyName = c.CompanyName,
                City = c.City,
                State = c.State,
                Status = c.Status,
                CreatedDate = c.CreatedDate,
                IsActive = c.IsActive
            }),
            Search = search,
            SortBy = sortBy,
            Ascending = ascending,
            CurrentPage = page,
            TotalCount = totalCount
        };

        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var customer = await _customerService.GetByIdWithDetailsAsync(id, userId, role);
        if (customer == null)
            return NotFound();

        // Customer 360 View: Build unified chronological timeline
        var timeline = new List<CustomerTimelineItemViewModel>();

        foreach (var act in customer.Activities)
        {
            timeline.Add(new CustomerTimelineItemViewModel
            {
                Date = act.ActivityDate,
                ItemType = "Activity",
                Title = $"{act.ActivityType}: {act.Subject}",
                Description = act.Description ?? string.Empty,
                BadgeClass = "primary",
                Icon = act.ActivityType switch { "Call" => "bi-telephone", "Meeting" => "bi-people", "Email" => "bi-envelope", _ => "bi-check2-square" },
                UserName = act.AssignedTo,
                RecordId = act.ActivityId.ToString()
            });
        }

        foreach (var fu in customer.FollowUps)
        {
            timeline.Add(new CustomerTimelineItemViewModel
            {
                Date = fu.FollowUpDate,
                ItemType = "FollowUp",
                Title = $"Follow-Up ({fu.FollowUpType}): {fu.Status}",
                Description = fu.Remarks ?? string.Empty,
                BadgeClass = fu.Status == "Completed" ? "success" : (fu.Status == "Missed" ? "danger" : "info"),
                Icon = "bi-calendar-event",
                UserName = fu.AssignedTo,
                RecordId = fu.FollowUpId.ToString()
            });
        }

        // Fetch audit logs for this customer
        var auditLogs = (await _auditRepo.GetAllAsync())
            .Where(a => a.EntityName == "Customer" && a.RecordId == id.ToString())
            .OrderByDescending(a => a.CreatedDate)
            .ToList();

        foreach (var audit in auditLogs)
        {
            timeline.Add(new CustomerTimelineItemViewModel
            {
                Date = audit.CreatedDate,
                ItemType = "AuditLog",
                Title = $"System Audit: {audit.Action}",
                Description = !string.IsNullOrEmpty(audit.NewValue) ? $"Changed: {audit.NewValue}" : string.Empty,
                BadgeClass = "secondary",
                Icon = "bi-clock-history",
                UserName = audit.UserId,
                RecordId = audit.AuditLogId.ToString()
            });
        }

        var model = new CustomerDetailsViewModel
        {
            Customer = customer,
            RelatedLeads = customer.Leads.ToList(),
            RelatedOpportunities = customer.Opportunities.ToList(),
            RelatedFollowUps = customer.FollowUps.ToList(),
            RelatedActivities = customer.Activities.ToList(),
            UnifiedTimeline = timeline.OrderByDescending(t => t.Date).ToList(),
            AuditLogs = auditLogs
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CustomerCreateEditViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CustomerCreateEditViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var customer = new Customer
        {
            CustomerName = model.CustomerName,
            Email = model.Email,
            Phone = model.Phone,
            CompanyName = model.CompanyName,
            Address = model.Address,
            City = model.City,
            State = model.State,
            Status = model.Status
        };

        var (success, error, created) = await _customerService.CreateAsync(customer, userId, role);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to create customer.");
            return View(model);
        }

        TempData["SuccessMessage"] = $"Customer '{created!.CustomerName}' ({created.CustomerCode}) created successfully.";
        return RedirectToAction("Details", new { id = created.CustomerId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var customer = await _customerService.GetByIdAsync(id, userId, role);
        if (customer == null)
            return NotFound();

        var model = new CustomerCreateEditViewModel
        {
            CustomerId = customer.CustomerId,
            CustomerName = customer.CustomerName,
            Email = customer.Email,
            Phone = customer.Phone,
            CompanyName = customer.CompanyName,
            Address = customer.Address,
            City = customer.City,
            State = customer.State,
            Status = customer.Status
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(CustomerCreateEditViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var customer = new Customer
        {
            CustomerId = model.CustomerId,
            CustomerName = model.CustomerName,
            Email = model.Email,
            Phone = model.Phone,
            CompanyName = model.CompanyName,
            Address = model.Address,
            City = model.City,
            State = model.State,
            Status = model.Status
        };

        var (success, error) = await _customerService.UpdateAsync(customer, userId, role);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to update customer.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Customer updated successfully.";
        return RedirectToAction("Details", new { id = model.CustomerId });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var (success, error) = await _customerService.DeleteAsync(id, userId, role);
        if (!success)
        {
            TempData["ErrorMessage"] = error ?? "Failed to delete customer.";
            return RedirectToAction("Index");
        }

        TempData["SuccessMessage"] = "Customer deactivated successfully.";
        return RedirectToAction("Index");
    }
}
