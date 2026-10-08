using System.Security.Claims;
using AcxiomCRM.Models.ViewModels;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Controllers;

public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index(string? filter = "month", DateTime? customStart = null, DateTime? customEnd = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        DateTime? startDate = filter switch
        {
            "today" => DateTime.Today,
            "week" => DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek),
            "month" => new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
            "custom" => customStart,
            _ => null
        };
        DateTime? endDate = filter == "custom" ? customEnd : null;

        var data = await _dashboardService.GetDashboardDataAsync(userId, role, startDate, endDate);
        ViewBag.ActiveFilter = filter;
        ViewBag.UserRole = role;

        return View(data);
    }

    [HttpGet]
    public async Task<IActionResult> GetChartData(string? filter = "month")
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var data = await _dashboardService.GetDashboardDataAsync(userId, role);
        return Json(new
        {
            leadStatus = data.LeadsByStatus,
            pipelineStage = data.OpportunitiesByStage,
            monthlySales = data.MonthlySales
        });
    }
}
