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
public class LeadsController : ControllerBase
{
    private readonly ILeadService _leadService;

    public LeadsController(ILeadService leadService)
    {
        _leadService = leadService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var (items, total) = await _leadService.GetPagedAsync(userId, role, search, status, page, pageSize);

        var dtos = items.Select(l => new LeadDto
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
            AssignedTo = l.AssignedTo
        });

        return Ok(new { data = dtos, total, page, pageSize });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLeadDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var lead = new Lead
        {
            LeadName = dto.LeadName,
            Email = dto.Email,
            Phone = dto.Phone,
            CompanyName = dto.CompanyName,
            Source = dto.Source,
            Status = dto.Status,
            ExpectedValue = dto.ExpectedValue,
            AssignedTo = dto.AssignedTo ?? userId
        };

        var (success, error, created) = await _leadService.CreateAsync(lead, userId, role);
        if (!success)
            return BadRequest(new ProblemDetails { Status = 400, Title = "Failed to create lead", Detail = error });

        return CreatedAtAction(nameof(GetAll), new { id = created!.LeadId }, new LeadDto
        {
            LeadId = created.LeadId,
            LeadCode = created.LeadCode,
            LeadName = created.LeadName,
            Email = created.Email,
            Phone = created.Phone,
            CompanyName = created.CompanyName,
            Source = created.Source,
            Status = created.Status,
            ExpectedValue = created.ExpectedValue,
            LeadScore = created.LeadScore,
            Priority = created.Priority,
            CreatedDate = created.CreatedDate,
            AssignedTo = created.AssignedTo
        });
    }
}
