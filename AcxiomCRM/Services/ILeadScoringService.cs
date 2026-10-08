using AcxiomCRM.Models.Entities;

namespace AcxiomCRM.Services;

/// <summary>
/// Calculates lead score and priority based on business rules.
/// </summary>
public interface ILeadScoringService
{
    void CalculateScore(Lead lead);
}
