using AcxiomCRM.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditService _auditService;

    public UserService(UserManager<ApplicationUser> userManager, IAuditService auditService)
    {
        _userManager = userManager;
        _auditService = auditService;
    }

    public async Task<IEnumerable<(ApplicationUser User, string Role)>> GetAllUsersWithRolesAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        var result = new List<(ApplicationUser, string)>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add((user, roles.FirstOrDefault() ?? "None"));
        }

        return result;
    }

    public async Task<(ApplicationUser? User, string? Role)> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return (null, null);
        var roles = await _userManager.GetRolesAsync(user);
        return (user, roles.FirstOrDefault());
    }

    public async Task<(bool Success, string? Error)> UpdateUserAsync(string userId, string fullName, string email, bool isActive, string? newRole)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return (false, "User not found.");

        user.FullName = fullName;
        user.Email = email;
        user.UserName = email;
        user.IsActive = isActive;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return (false, string.Join("; ", result.Errors.Select(e => e.Description)));

        if (!string.IsNullOrEmpty(newRole))
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(newRole))
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, newRole);
            }
        }

        await _auditService.LogAsync(userId, "UpdateUser", "User", userId, newValue: new { fullName, email, isActive, newRole });
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UnlockUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return (false, "User not found.");

        user.LockoutEnd = null;
        user.AccessFailedCount = 0;
        await _userManager.UpdateAsync(user);

        await _auditService.LogAsync(userId, "Unlock", "User", userId);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ResetPasswordAsync(string userId, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return (false, "User not found.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (!result.Succeeded) return (false, string.Join("; ", result.Errors.Select(e => e.Description)));

        await _auditService.LogAsync(userId, "ResetPassword", "User", userId);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ToggleUserActiveAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return (false, "User not found.");

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);

        await _auditService.LogAsync(userId, "ToggleActive", "User", userId, newValue: new { IsActive = user.IsActive });
        return (true, null);
    }
}
