using System.Security.Claims;
using AcxiomCRM.Models.Dtos;
using AcxiomCRM.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin,Manager")]
public class ReportsController : ControllerBase
{
    private readonly IOpportunityRepository _oppRepo;

    public ReportsController(IOpportunityRepository oppRepo)
    {
        _oppRepo = oppRepo;
    }

    [HttpGet("pipeline")]
    public async Task<IActionResult> GetPipelineReport()
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "Manager";
        var opps = await _oppRepo.GetOpportunitiesQuery(null).Where(o => o.Status == "Open").ToListAsync();

        var stages = new[] { "Qualification", "Proposal", "Negotiation" };
        var report = stages.Select(s => new PipelineReportDto
        {
            Stage = s,
            DealCount = opps.Count(o => o.Stage == s),
            TotalAmount = opps.Where(o => o.Stage == s).Sum(o => o.Amount),
            WeightedAmount = opps.Where(o => o.Stage == s).Sum(o => o.Amount * o.Probability / 100)
        }).ToList();

        return Ok(report);
    }
}
