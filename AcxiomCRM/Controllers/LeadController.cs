using System.Security.Claims;
using AcxiomCRM.Models.Entities;
using AcxiomCRM.Models.ViewModels;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Controllers;

public class LeadController : Controller
{
    private readonly ILeadService _leadService;
    private readonly UserManager<ApplicationUser> _userManager;

    public LeadController(ILeadService leadService, UserManager<ApplicationUser> userManager)
    {
        _leadService = leadService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, string? statusFilter, int page = 1, string? sortBy = null, bool ascending = true)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var (items, totalCount) = await _leadService.GetPagedAsync(userId, role, search, statusFilter, page, 10, sortBy, ascending);

        var model = new LeadListViewModel
        {
            Leads = items.Select(l => new LeadItemViewModel
            {
                LeadId = l.LeadId,
                LeadCode = l.LeadCode,
                LeadName = l.LeadName,
                Email = l.Email,
                Phone = l.Phone,
                CompanyName = l.CompanyName,
                Source = l.Source,
                Status = l.Status,
                ExpectedValue = l.ExpectedValue,
                LeadScore = l.LeadScore,
                Priority = l.Priority,
                CreatedDate = l.CreatedDate,
                AssignedTo = l.AssignedTo,
                AssignedUserName = l.AssignedUser?.FullName ?? l.AssignedTo
            }),
            Search = search,
            StatusFilter = statusFilter,
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

        var lead = await _leadService.GetByIdWithDetailsAsync(id, userId, role);
        if (lead == null)
            return NotFound();

        var model = new LeadDetailsViewModel
        {
            Lead = lead,
            ValidNextTransitions = _leadService.GetValidTransitions(lead.Status),
            RelatedOpportunities = lead.Opportunities.ToList(),
            RelatedFollowUps = lead.FollowUps.ToList(),
            RelatedActivities = lead.Activities.ToList()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";
        if (role == "Admin" || role == "Manager")
        {
            ViewBag.Users = await _userManager.GetUsersInRoleAsync("SalesExecutive");
        }
        return View(new LeadCreateEditViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(LeadCreateEditViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var lead = new Lead
        {
            LeadName = model.LeadName,
            Email = model.Email,
            Phone = model.Phone,
            CompanyName = model.CompanyName,
            Source = model.Source,
            ExpectedValue = model.ExpectedValue,
            AssignedTo = model.AssignedTo ?? userId
        };

        var (success, error, created) = await _leadService.CreateAsync(lead, userId, role);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to create lead.");
            return View(model);
        }

        TempData["SuccessMessage"] = $"Lead '{created!.LeadName}' ({created.LeadCode}) created successfully.";
        return RedirectToAction("Details", new { id = created.LeadId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var lead = await _leadService.GetByIdAsync(id, userId, role);
        if (lead == null)
            return NotFound();

        var model = new LeadCreateEditViewModel
        {
            LeadId = lead.LeadId,
            LeadName = lead.LeadName,
            Email = lead.Email,
            Phone = lead.Phone,
            CompanyName = lead.CompanyName,
            Source = lead.Source,
            Status = lead.Status,
            ExpectedValue = lead.ExpectedValue,
            AssignedTo = lead.AssignedTo
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(LeadCreateEditViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var lead = new Lead
        {
            LeadId = model.LeadId,
            LeadName = model.LeadName,
            Email = model.Email,
            Phone = model.Phone,
            CompanyName = model.CompanyName,
            Source = model.Source,
            ExpectedValue = model.ExpectedValue,
            AssignedTo = model.AssignedTo ?? userId
        };

        var (success, error) = await _leadService.UpdateAsync(lead, userId, role);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to update lead.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Lead updated successfully.";
        return RedirectToAction("Details", new { id = model.LeadId });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var (success, error) = await _leadService.DeleteAsync(id, userId, role);
        if (!success)
        {
            TempData["ErrorMessage"] = error ?? "Failed to delete lead.";
            return RedirectToAction("Index");
        }

        TempData["SuccessMessage"] = "Lead deleted successfully.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> ChangeStatus(int id, string newStatus)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var (success, error) = await _leadService.ChangeStatusAsync(id, newStatus, userId, role);
        if (!success)
        {
            TempData["ErrorMessage"] = error;
        }
        else
        {
            TempData["SuccessMessage"] = $"Lead status successfully changed to '{newStatus}'.";
        }

        return RedirectToAction("Details", new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Convert(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var lead = await _leadService.GetByIdAsync(id, userId, role);
        if (lead == null)
            return NotFound();

        if (lead.Status != "Qualified")
        {
            TempData["ErrorMessage"] = "Only Qualified leads can be converted.";
            return RedirectToAction("Details", new { id });
        }

        var model = new LeadConvertViewModel
        {
            LeadId = lead.LeadId,
            LeadCode = lead.LeadCode,
            LeadName = lead.LeadName,
            Email = lead.Email,
            Phone = lead.Phone,
            CompanyName = lead.CompanyName,
            OpportunityName = $"Opp-{lead.LeadName}",
            Amount = lead.ExpectedValue
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Convert(LeadConvertViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var (success, error, customerId) = await _leadService.ConvertAsync(
            model.LeadId,
            model.CreateOpportunity ? model.OpportunityName : null,
            model.CreateOpportunity ? model.Amount : null,
            userId,
            role);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Lead conversion failed.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Lead successfully converted to Customer!";
        return RedirectToAction("Details", "Customer", new { id = customerId });
    }
}
