using System.Security.Claims;
using AcxiomCRM.Models.Entities;
using AcxiomCRM.Models.ViewModels;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Controllers;

public class ActivitiesController : Controller
{
    private readonly IActivityService _activityService;
    private readonly ICustomerService _customerService;

    public ActivitiesController(IActivityService activityService, ICustomerService customerService)
    {
        _activityService = activityService;
        _customerService = customerService;
    }

    public async Task<IActionResult> Index(string? typeFilter, int page = 1, string? sortBy = null, bool ascending = true)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var (items, totalCount) = await _activityService.GetPagedAsync(userId, role, typeFilter, page, 10, sortBy, ascending);

        var model = new ActivityListViewModel
        {
            Activities = items.Select(a => new ActivityItemViewModel
            {
                ActivityId = a.ActivityId,
                ActivityType = a.ActivityType,
                Subject = a.Subject,
                Description = a.Description,
                ActivityDate = a.ActivityDate,
                CustomerId = a.CustomerId,
                CustomerName = a.Customer?.CustomerName,
                LeadId = a.LeadId,
                LeadName = a.Lead?.LeadName,
                AssignedTo = a.AssignedTo,
                AssignedUserName = a.AssignedUser?.FullName ?? a.AssignedTo,
                Status = a.Status
            }),
            TypeFilter = typeFilter,
            SortBy = sortBy,
            Ascending = ascending,
            CurrentPage = page,
            TotalCount = totalCount
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? customerId = null, int? leadId = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        ViewBag.Customers = await _customerService.GetAllActiveAsync(userId, role);

        return View(new ActivityCreateEditViewModel
        {
            CustomerId = customerId,
            LeadId = leadId,
            ActivityDate = DateTime.Now
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(ActivityCreateEditViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        if (!ModelState.IsValid)
        {
            ViewBag.Customers = await _customerService.GetAllActiveAsync(userId, role);
            return View(model);
        }

        var activity = new Activity
        {
            ActivityType = model.ActivityType,
            Subject = model.Subject,
            Description = model.Description,
            ActivityDate = model.ActivityDate,
            CustomerId = model.CustomerId,
            LeadId = model.LeadId,
            Status = model.Status,
            AssignedTo = model.AssignedTo ?? userId
        };

        var (success, error, created) = await _activityService.CreateAsync(activity, userId, role);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to log activity.");
            ViewBag.Customers = await _customerService.GetAllActiveAsync(userId, role);
            return View(model);
        }

        TempData["SuccessMessage"] = $"{created!.ActivityType} logged successfully.";
        return RedirectToAction("Index");
    }
}
