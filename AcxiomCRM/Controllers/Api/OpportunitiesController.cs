using System.Security.Claims;
using AcxiomCRM.Models.Dtos;
using AcxiomCRM.Models.Entities;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
[IgnoreAntiforgeryToken]
public class OpportunitiesController : ControllerBase
{
    private readonly IOpportunityService _oppService;
    private readonly IDealHealthService _healthService;

    public OpportunitiesController(IOpportunityService oppService, IDealHealthService healthService)
    {
        _oppService = oppService;
        _healthService = healthService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? stage, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var (items, total) = await _oppService.GetPagedAsync(userId, role, search, stage, page, pageSize);

        var list = new List<OpportunityDto>();
        foreach (var o in items)
        {
            var health = await _healthService.CalculateHealthScoreAsync(o);
            list.Add(new OpportunityDto
            {
                OpportunityId = o.OpportunityId,
                OpportunityName = o.OpportunityName,
                CustomerId = o.CustomerId,
                CustomerName = o.Customer?.CustomerName,
                LeadId = o.LeadId,
                Amount = o.Amount,
                Stage = o.Stage,
                Probability = o.Probability,
                ExpectedCloseDate = o.ExpectedCloseDate,
                Status = o.Status,
                DealHealthScore = health,
                CreatedDate = o.CreatedDate,
                AssignedTo = o.AssignedTo
            });
        }

        return Ok(new { data = list, total, page, pageSize });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOpportunityDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var opp = new Opportunity
        {
            OpportunityName = dto.OpportunityName,
            CustomerId = dto.CustomerId,
            LeadId = dto.LeadId,
            Amount = dto.Amount,
            Stage = dto.Stage,
            Probability = dto.Probability,
            ExpectedCloseDate = dto.ExpectedCloseDate,
            Status = dto.Status,
            AssignedTo = dto.AssignedTo ?? userId
        };

        var (success, error, created) = await _oppService.CreateAsync(opp, userId, role);
        if (!success)
            return BadRequest(new ProblemDetails { Status = 400, Title = "Failed to create opportunity", Detail = error });

        return CreatedAtAction(nameof(GetAll), new { id = created!.OpportunityId }, new OpportunityDto
        {
            OpportunityId = created.OpportunityId,
            OpportunityName = created.OpportunityName,
            CustomerId = created.CustomerId,
            LeadId = created.LeadId,
            Amount = created.Amount,
            Stage = created.Stage,
            Probability = created.Probability,
            ExpectedCloseDate = created.ExpectedCloseDate,
            Status = created.Status,
            CreatedDate = created.CreatedDate,
            AssignedTo = created.AssignedTo
        });
    }
}
