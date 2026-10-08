using AcxiomCRM.Models.Entities;
using AcxiomCRM.Models.ViewModels;
using AcxiomCRM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IUserService _userService;
    private readonly IAuditService _auditService;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IUserService userService,
        IAuditService auditService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _userService = userService;
        _auditService = auditService;
    }

    public async Task<IActionResult> Index()
    {
        var usersWithRoles = await _userService.GetAllUsersWithRolesAsync();
        var model = new UserListViewModel
        {
            Users = usersWithRoles.Select(ur => new UserItemViewModel
            {
                Id = ur.User.Id,
                FullName = ur.User.FullName,
                Email = ur.User.Email ?? string.Empty,
                Role = ur.Role,
                IsActive = ur.User.IsActive,
                IsLockedOut = ur.User.LockoutEnd.HasValue && ur.User.LockoutEnd > DateTimeOffset.UtcNow,
                LockoutEnd = ur.User.LockoutEnd?.UtcDateTime,
                CreatedDate = ur.User.CreatedDate,
                LastLoginDate = ur.User.LastLoginDate
            }).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var (user, role) = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound();

        ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

        return View(new UserEditViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Role = role ?? "SalesExecutive",
            IsActive = user.IsActive
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(UserEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(model);
        }

        var (success, error) = await _userService.UpdateUserAsync(model.Id, model.FullName, model.Email, model.IsActive, model.Role);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to update user.");
            ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = "User updated successfully.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Unlock(string id)
    {
        var (success, error) = await _userService.UnlockUserAsync(id);
        if (!success)
            TempData["ErrorMessage"] = error ?? "Failed to unlock user.";
        else
            TempData["SuccessMessage"] = "User account unlocked.";

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(string id)
    {
        var (success, error) = await _userService.ToggleUserActiveAsync(id);
        if (!success)
            TempData["ErrorMessage"] = error ?? "Failed to toggle status.";
        else
            TempData["SuccessMessage"] = "User status updated.";

        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> ResetPassword(string id)
    {
        var (user, _) = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound();

        return View(new UserResetPasswordViewModel
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty
        });
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(UserResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var (success, error) = await _userService.ResetPasswordAsync(model.UserId, model.NewPassword);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to reset password.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Password reset successfully.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Permissions()
    {
        var matrix = new PermissionsMatrixViewModel
        {
            Permissions = new List<PermissionRow>
            {
                new() { Module = "Dashboard", AdminAccess = "Full Access", ManagerAccess = "Team Pipeline", SalesExecutiveAccess = "Own Records" },
                new() { Module = "Customers", AdminAccess = "Full Access", ManagerAccess = "Full Access", SalesExecutiveAccess = "Own Created" },
                new() { Module = "Leads", AdminAccess = "Full Access", ManagerAccess = "Full Access", SalesExecutiveAccess = "Own Assigned" },
                new() { Module = "Opportunities & Kanban", AdminAccess = "Full Access", ManagerAccess = "Full Access", SalesExecutiveAccess = "Own Assigned" },
                new() { Module = "Follow-Ups", AdminAccess = "Full Access", ManagerAccess = "Full Access", SalesExecutiveAccess = "Own Assigned" },
                new() { Module = "Activities", AdminAccess = "Full Access", ManagerAccess = "Full Access", SalesExecutiveAccess = "Own Assigned" },
                new() { Module = "User Management", AdminAccess = "Full Access", ManagerAccess = "Limited View", SalesExecutiveAccess = "No Access" },
                new() { Module = "Role & Permissions", AdminAccess = "Full Access", ManagerAccess = "No Access", SalesExecutiveAccess = "No Access" },
                new() { Module = "Audit Logs", AdminAccess = "Full Access & Diff Viewer", ManagerAccess = "Limited Read-Only", SalesExecutiveAccess = "No Access" },
                new() { Module = "Reports", AdminAccess = "All Reports + Export", ManagerAccess = "Management Reports + Export", SalesExecutiveAccess = "Own Reports + Export" },
                new() { Module = "REST API", AdminAccess = "Full Access", ManagerAccess = "Scoped Access", SalesExecutiveAccess = "Scoped Access" }
            }
        };

        return View(matrix);
    }
}
