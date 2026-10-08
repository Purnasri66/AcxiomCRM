using System.Security.Claims;
using AcxiomCRM.Models.Entities;
using AcxiomCRM.Models.ViewModels;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Controllers;

public class OpportunityController : Controller
{
    private readonly IOpportunityService _opportunityService;
    private readonly ICustomerService _customerService;
    private readonly IDealHealthService _healthService;

    public OpportunityController(
        IOpportunityService opportunityService,
        ICustomerService customerService,
        IDealHealthService healthService)
    {
        _opportunityService = opportunityService;
        _customerService = customerService;
        _healthService = healthService;
    }

    public async Task<IActionResult> Index(string? search, string? stageFilter, int page = 1, string? sortBy = null, bool ascending = true)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var (items, totalCount) = await _opportunityService.GetPagedAsync(userId, role, search, stageFilter, page, 10, sortBy, ascending);

        var list = new List<OpportunityItemViewModel>();
        foreach (var o in items)
        {
            var healthScore = await _healthService.CalculateHealthScoreAsync(o);
            list.Add(new OpportunityItemViewModel
            {
                OpportunityId = o.OpportunityId,
                OpportunityName = o.OpportunityName,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer?.CustomerName ?? "Unknown",
                Amount = o.Amount,
                Stage = o.Stage,
                Probability = o.Probability,
                ExpectedCloseDate = o.ExpectedCloseDate,
                Status = o.Status,
                DealHealthScore = healthScore,
                DealHealthColor = _healthService.GetHealthColor(healthScore),
                DealHealthLabel = _healthService.GetHealthLabel(healthScore),
                AssignedTo = o.AssignedTo,
                AssignedUserName = o.AssignedUser?.FullName ?? o.AssignedTo
            });
        }

        var model = new OpportunityListViewModel
        {
            Opportunities = list,
            Search = search,
            StageFilter = stageFilter,
            SortBy = sortBy,
            Ascending = ascending,
            CurrentPage = page,
            TotalCount = totalCount
        };

        return View(model);
    }

    public async Task<IActionResult> Kanban()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var kanbanData = await _opportunityService.GetKanbanDataAsync(userId, role);
        var viewModel = new KanbanBoardViewModel();

        foreach (var kvp in kanbanData)
        {
            viewModel.Stages[kvp.Key] = kvp.Value.Select(o => new OpportunityItemViewModel
            {
                OpportunityId = o.OpportunityId,
                OpportunityName = o.OpportunityName,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer?.CustomerName ?? "Unknown",
                Amount = o.Amount,
                Stage = o.Stage,
                Probability = o.Probability,
                ExpectedCloseDate = o.ExpectedCloseDate,
                Status = o.Status,
                DealHealthScore = o.DealHealthScore,
                DealHealthColor = _healthService.GetHealthColor(o.DealHealthScore),
                DealHealthLabel = _healthService.GetHealthLabel(o.DealHealthScore),
                AssignedTo = o.AssignedTo,
                AssignedUserName = o.AssignedUser?.FullName ?? o.AssignedTo
            }).ToList();
        }

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStage([FromBody] UpdateStageRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var (success, error) = await _opportunityService.ChangeStageAsync(request.OpportunityId, request.NewStage, userId, role);
        if (!success)
            return BadRequest(new { success = false, message = error });

        return Ok(new { success = true, message = "Stage updated successfully." });
    }

    public class UpdateStageRequest
    {
        public int OpportunityId { get; set; }
        public string NewStage { get; set; } = string.Empty;
    }

    public async Task<IActionResult> Details(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var opp = await _opportunityService.GetByIdWithDetailsAsync(id, userId, role);
        if (opp == null)
            return NotFound();

        var healthScore = await _healthService.CalculateHealthScoreAsync(opp);

        var model = new OpportunityDetailsViewModel
        {
            Opportunity = opp,
            DealHealthScore = healthScore,
            DealHealthColor = _healthService.GetHealthColor(healthScore),
            DealHealthLabel = _healthService.GetHealthLabel(healthScore),
            RelatedFollowUps = opp.FollowUps.ToList(),
            RelatedActivities = opp.Activities.ToList()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? customerId = null, int? leadId = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        ViewBag.Customers = await _customerService.GetAllActiveAsync(userId, role);

        return View(new OpportunityCreateEditViewModel
        {
            CustomerId = customerId ?? 0,
            LeadId = leadId,
            ExpectedCloseDate = DateTime.Today.AddDays(30)
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(OpportunityCreateEditViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        if (!ModelState.IsValid)
        {
            ViewBag.Customers = await _customerService.GetAllActiveAsync(userId, role);
            return View(model);
        }

        var opp = new Opportunity
        {
            OpportunityName = model.OpportunityName,
            CustomerId = model.CustomerId,
            LeadId = model.LeadId,
            Amount = model.Amount,
            Stage = model.Stage,
            Probability = model.Probability,
            ExpectedCloseDate = model.ExpectedCloseDate,
            Status = model.Status,
            AssignedTo = model.AssignedTo ?? userId
        };

        var (success, error, created) = await _opportunityService.CreateAsync(opp, userId, role);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to create opportunity.");
            ViewBag.Customers = await _customerService.GetAllActiveAsync(userId, role);
            return View(model);
        }

        TempData["SuccessMessage"] = $"Opportunity '{created!.OpportunityName}' created successfully.";
        return RedirectToAction("Details", new { id = created.OpportunityId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var opp = await _opportunityService.GetByIdAsync(id, userId, role);
        if (opp == null)
            return NotFound();

        ViewBag.Customers = await _customerService.GetAllActiveAsync(userId, role);

        var model = new OpportunityCreateEditViewModel
        {
            OpportunityId = opp.OpportunityId,
            OpportunityName = opp.OpportunityName,
            CustomerId = opp.CustomerId,
            LeadId = opp.LeadId,
            Amount = opp.Amount,
            Stage = opp.Stage,
            Probability = opp.Probability,
            ExpectedCloseDate = opp.ExpectedCloseDate,
            Status = opp.Status,
            AssignedTo = opp.AssignedTo
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(OpportunityCreateEditViewModel model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        if (!ModelState.IsValid)
        {
            ViewBag.Customers = await _customerService.GetAllActiveAsync(userId, role);
            return View(model);
        }

        var opp = new Opportunity
        {
            OpportunityId = model.OpportunityId,
            OpportunityName = model.OpportunityName,
            CustomerId = model.CustomerId,
            LeadId = model.LeadId,
            Amount = model.Amount,
            Stage = model.Stage,
            Probability = model.Probability,
            ExpectedCloseDate = model.ExpectedCloseDate,
            Status = model.Status,
            AssignedTo = model.AssignedTo ?? userId
        };

        var (success, error) = await _opportunityService.UpdateAsync(opp, userId, role);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to update opportunity.");
            ViewBag.Customers = await _customerService.GetAllActiveAsync(userId, role);
            return View(model);
        }

        TempData["SuccessMessage"] = "Opportunity updated successfully.";
        return RedirectToAction("Details", new { id = model.OpportunityId });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var (success, error) = await _opportunityService.DeleteAsync(id, userId, role);
        if (!success)
        {
            TempData["ErrorMessage"] = error ?? "Failed to delete opportunity.";
            return RedirectToAction("Index");
        }

        TempData["SuccessMessage"] = "Opportunity deleted successfully.";
        return RedirectToAction("Index");
    }
}
