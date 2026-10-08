using AcxiomCRM.Models.Entities;

namespace AcxiomCRM.Services;

/// <summary>
/// Calculates deal health score (0-100) for opportunities.
/// </summary>
public interface IDealHealthService
{
    /// <summary>
    /// Computes health from probability, days to close, last activity, overdue follow-ups.
    /// </summary>
    Task<int> CalculateHealthScoreAsync(Opportunity opportunity);
    string GetHealthLabel(int score);
    string GetHealthColor(int score);
}
