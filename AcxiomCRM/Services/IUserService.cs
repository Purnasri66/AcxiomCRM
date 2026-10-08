using AcxiomCRM.Models.Entities;

namespace AcxiomCRM.Services;

/// <summary>
/// Service for user administration (Admin only).
/// </summary>
public interface IUserService
{
    Task<IEnumerable<(ApplicationUser User, string Role)>> GetAllUsersWithRolesAsync();
    Task<(ApplicationUser? User, string? Role)> GetUserByIdAsync(string userId);
    Task<(bool Success, string? Error)> UpdateUserAsync(string userId, string fullName, string email, bool isActive, string? newRole);
    Task<(bool Success, string? Error)> UnlockUserAsync(string userId);
    Task<(bool Success, string? Error)> ResetPasswordAsync(string userId, string newPassword);
    Task<(bool Success, string? Error)> ToggleUserActiveAsync(string userId);
}
