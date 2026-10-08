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
public class FollowUpsController : ControllerBase
{
    private readonly IFollowUpService _followUpService;

    public FollowUpsController(IFollowUpService followUpService)
    {
        _followUpService = followUpService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var (items, total) = await _followUpService.GetPagedAsync(userId, role, status, page, pageSize);

        var dtos = items.Select(f => new FollowUpDto
        {
            FollowUpId = f.FollowUpId,
            CustomerId = f.CustomerId,
            CustomerName = f.Customer?.CustomerName,
            LeadId = f.LeadId,
            LeadName = f.Lead?.LeadName,
            OpportunityId = f.OpportunityId,
            FollowUpDate = f.FollowUpDate,
            FollowUpType = f.FollowUpType,
            Remarks = f.Remarks,
            Status = f.Status,
            AssignedTo = f.AssignedTo,
            CreatedDate = f.CreatedDate
        });

        return Ok(new { data = dtos, total, page, pageSize });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFollowUpDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var followUp = new FollowUp
        {
            CustomerId = dto.CustomerId,
            LeadId = dto.LeadId,
            OpportunityId = dto.OpportunityId,
            FollowUpDate = dto.FollowUpDate,
            FollowUpType = dto.FollowUpType,
            Remarks = dto.Remarks,
            Status = "Planned",
            AssignedTo = dto.AssignedTo ?? userId
        };

        var (success, error, created) = await _followUpService.CreateAsync(followUp, userId, role);
        if (!success)
            return BadRequest(new ProblemDetails { Status = 400, Title = "Failed to create follow-up", Detail = error });

        return CreatedAtAction(nameof(GetAll), new { id = created!.FollowUpId }, new FollowUpDto
        {
            FollowUpId = created.FollowUpId,
            CustomerId = created.CustomerId,
            LeadId = created.LeadId,
            OpportunityId = created.OpportunityId,
            FollowUpDate = created.FollowUpDate,
            FollowUpType = created.FollowUpType,
            Remarks = created.Remarks,
            Status = created.Status,
            AssignedTo = created.AssignedTo,
            CreatedDate = created.CreatedDate
        });
    }
}
