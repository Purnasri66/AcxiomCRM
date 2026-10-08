using System.Security.Claims;
using AcxiomCRM.Models.Entities;
using AcxiomCRM.Models.ViewModels;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Controllers;

public class FollowUpsController : Controller
{
    private readonly IFollowUpService _followUpService;
    private readonly ICustomerService _customerService;

    public FollowUpsController(IFollowUpService followUpService, ICustomerService customerService)
    {
        _followUpService = followUpService;
        _customerService = customerService;
    }

    public async Task<IActionResult> Index(string? statusFilter, int page = 1, string? sortBy = null, bool ascending = true)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var (items, totalCount) = await _followUpService.GetPagedAsync(userId, role, statusFilter, page, 10, sortBy, ascending);

        var model = new FollowUpListViewModel
        {
            FollowUps = items.Select(f => new FollowUpItemViewModel
            {
                FollowUpId = f.FollowUpId,
                FollowUpDate = f.FollowUpDate,
                FollowUpType = f.FollowUpType,
                Remarks = f.Remarks,
                Status = f.Status,
                CustomerId = f.CustomerId,
                CustomerName = f.Customer?.CustomerName,
                LeadId = f.LeadId,
                LeadName = f.Lead?.LeadName,
                OpportunityId = f.OpportunityId,
                OpportunityName = f.Opportunity?.OpportunityName,
                AssignedTo = f.AssignedTo,
                AssignedUserName = f.AssignedUser?.FullName ?? f.AssignedTo
            }),
            StatusFilter = statusFilter,
            SortBy = sortBy,
            Ascending = ascending,
            CurrentPage = page,
            TotalCount = totalCount
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? customerId = null, int? leadId = null, int? opportunityId = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        ViewBag.Customers = await _customerService.GetAllActiveAsync(userId, role);

        return View(new FollowUpCreateEditViewModel
        {
            CustomerId = customerId,
            LeadId = leadId,
            OpportunityId = opportunityId,
            FollowUpDate = DateTime.Today.AddDays(1)
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(FollowUpCreateEditViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        if (!ModelState.IsValid)
        {
            ViewBag.Customers = await _customerService.GetAllActiveAsync(userId, role);
            return View(model);
        }

        var followUp = new FollowUp
        {
            CustomerId = model.CustomerId,
            LeadId = model.LeadId,
            OpportunityId = model.OpportunityId,
            FollowUpDate = model.FollowUpDate,
            FollowUpType = model.FollowUpType,
            Remarks = model.Remarks,
            Status = "Planned",
            AssignedTo = model.AssignedTo ?? userId
        };

        var (success, error, created) = await _followUpService.CreateAsync(followUp, userId, role);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to create follow-up.");
            ViewBag.Customers = await _customerService.GetAllActiveAsync(userId, role);
            return View(model);
        }

        TempData["SuccessMessage"] = "Follow-up scheduled successfully.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Complete(int id, string? remarks)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var (success, error) = await _followUpService.CompleteAsync(id, remarks, userId, role);
        if (!success)
            TempData["ErrorMessage"] = error ?? "Failed to complete follow-up.";
        else
            TempData["SuccessMessage"] = "Follow-up marked as completed.";

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Reschedule(int id, DateTime newDate)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var (success, error) = await _followUpService.RescheduleAsync(id, newDate, userId, role);
        if (!success)
            TempData["ErrorMessage"] = error ?? "Failed to reschedule follow-up.";
        else
            TempData["SuccessMessage"] = "Follow-up rescheduled successfully.";

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var (success, error) = await _followUpService.CancelAsync(id, userId, role);
        if (!success)
            TempData["ErrorMessage"] = error ?? "Failed to cancel follow-up.";
        else
            TempData["SuccessMessage"] = "Follow-up cancelled.";

        return RedirectToAction("Index");
    }
}
