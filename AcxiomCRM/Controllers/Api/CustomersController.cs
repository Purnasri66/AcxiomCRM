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
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var (items, total) = await _customerService.GetPagedAsync(userId, role, search, page, pageSize);

        var dtos = items.Select(c => new CustomerDto
        {
            CustomerId = c.CustomerId,
            CustomerCode = c.CustomerCode,
            CustomerName = c.CustomerName,
            Email = c.Email,
            Phone = c.Phone,
            CompanyName = c.CompanyName,
            Address = c.Address,
            City = c.City,
            State = c.State,
            Status = c.Status,
            CreatedDate = c.CreatedDate,
            CreatedBy = c.CreatedBy
        });

        return Ok(new { data = dtos, total, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var c = await _customerService.GetByIdAsync(id, userId, role);
        if (c == null)
            return NotFound(new ProblemDetails { Status = 404, Title = "Not Found", Detail = $"Customer with ID {id} not found or access denied." });

        return Ok(new CustomerDto
        {
            CustomerId = c.CustomerId,
            CustomerCode = c.CustomerCode,
            CustomerName = c.CustomerName,
            Email = c.Email,
            Phone = c.Phone,
            CompanyName = c.CompanyName,
            Address = c.Address,
            City = c.City,
            State = c.State,
            Status = c.Status,
            CreatedDate = c.CreatedDate,
            CreatedBy = c.CreatedBy
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var customer = new Customer
        {
            CustomerName = dto.CustomerName,
            Email = dto.Email,
            Phone = dto.Phone,
            CompanyName = dto.CompanyName,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            Status = dto.Status
        };

        var (success, error, created) = await _customerService.CreateAsync(customer, userId, role);
        if (!success)
        {
            if (error != null && error.Contains("already exists"))
                return Conflict(new ProblemDetails { Status = 409, Title = "Conflict", Detail = error });

            return BadRequest(new ProblemDetails { Status = 400, Title = "Creation Failed", Detail = error });
        }

        var resultDto = new CustomerDto
        {
            CustomerId = created!.CustomerId,
            CustomerCode = created.CustomerCode,
            CustomerName = created.CustomerName,
            Email = created.Email,
            Phone = created.Phone,
            CompanyName = created.CompanyName,
            Address = created.Address,
            City = created.City,
            State = created.State,
            Status = created.Status,
            CreatedDate = created.CreatedDate,
            CreatedBy = created.CreatedBy
        };

        return CreatedAtAction(nameof(GetById), new { id = created.CustomerId }, resultDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var customer = new Customer
        {
            CustomerId = id,
            CustomerName = dto.CustomerName,
            Email = dto.Email,
            Phone = dto.Phone,
            CompanyName = dto.CompanyName,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            Status = dto.Status
        };

        var (success, error) = await _customerService.UpdateAsync(customer, userId, role);
        if (!success)
        {
            if (error == "Customer not found.")
                return NotFound(new ProblemDetails { Status = 404, Title = "Not Found", Detail = error });
            if (error == "Access denied.")
                return StatusCode(403, new ProblemDetails { Status = 403, Title = "Forbidden", Detail = error });
            if (error != null && error.Contains("already exists"))
                return Conflict(new ProblemDetails { Status = 409, Title = "Conflict", Detail = error });

            return BadRequest(new ProblemDetails { Status = 400, Title = "Update Failed", Detail = error });
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";

        var (success, error) = await _customerService.DeleteAsync(id, userId, role);
        if (!success)
        {
            if (error == "Customer not found.")
                return NotFound(new ProblemDetails { Status = 404, Title = "Not Found", Detail = error });
            if (error == "Access denied.")
                return StatusCode(403, new ProblemDetails { Status = 403, Title = "Forbidden", Detail = error });

            return BadRequest(new ProblemDetails { Status = 400, Title = "Delete Failed", Detail = error });
        }

        return NoContent();
    }
}
