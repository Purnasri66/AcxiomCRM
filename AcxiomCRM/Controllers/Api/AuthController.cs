using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AcxiomCRM.Models.Dtos;
using AcxiomCRM.Models.Entities;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AcxiomCRM.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[IgnoreAntiforgeryToken]
public class AuthController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IAuditService _auditService;

    public AuthController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        IAuditService auditService)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _configuration = configuration;
        _auditService = auditService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ProblemDetails { Status = 400, Title = "Validation failed.", Detail = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !user.IsActive)
        {
            await _auditService.LogActionAsync("System", "FailedApiLogin", "Account", dto.Email, HttpContext.Connection.RemoteIpAddress?.ToString());
            return Unauthorized(new ProblemDetails { Status = 401, Title = "Unauthorized", Detail = "Invalid email or password." });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
        if (result.IsLockedOut)
        {
            await _auditService.LogActionAsync(user.Id, "Lockout", "Account", user.Id, HttpContext.Connection.RemoteIpAddress?.ToString());
            return StatusCode(StatusCodes.Status429TooManyRequests, new ProblemDetails { Status = 429, Title = "Account Locked", Detail = "Account is locked out. Please try again later." });
        }

        if (!result.Succeeded)
        {
            await _auditService.LogActionAsync(user.Id, "FailedApiLogin", "Account", user.Id, HttpContext.Connection.RemoteIpAddress?.ToString());
            return Unauthorized(new ProblemDetails { Status = 401, Title = "Unauthorized", Detail = "Invalid email or password." });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? "SalesExecutive";

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!);
        var expiresAt = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["JwtSettings:ExpiryInMinutes"] ?? "60"));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, primaryRole)
            }),
            Expires = expiresAt,
            Issuer = _configuration["JwtSettings:Issuer"],
            Audience = _configuration["JwtSettings:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtString = tokenHandler.WriteToken(token);

        await _auditService.LogActionAsync(user.Id, "ApiLoginSuccess", "Account", user.Id, HttpContext.Connection.RemoteIpAddress?.ToString());

        return Ok(new LoginResponseDto
        {
            Token = jwtString,
            Email = user.Email!,
            FullName = user.FullName,
            Role = primaryRole,
            ExpiresAt = expiresAt
        });
    }

    [HttpPost("logout")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != null)
        {
            await _auditService.LogActionAsync(userId, "ApiLogout", "Account", userId, HttpContext.Connection.RemoteIpAddress?.ToString());
        }
        return Ok(new { message = "Logged out successfully." });
    }
}
