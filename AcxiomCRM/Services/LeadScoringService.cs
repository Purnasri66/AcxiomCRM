using AcxiomCRM.Models.Entities;

namespace AcxiomCRM.Services;

public class LeadScoringService : ILeadScoringService
{
    public void CalculateScore(Lead lead)
    {
        int score = 0;

        // ExpectedValue factor (0-40 points)
        score += lead.ExpectedValue switch
        {
            >= 5000000 => 40,
            >= 1000000 => 30,
            >= 500000 => 20,
            >= 100000 => 10,
            _ => 5
        };

        // Source factor (0-30 points)
        score += lead.Source switch
        {
            "Referral" => 30,
            "Partner" => 25,
            "LinkedIn" => 20,
            "Website" => 15,
            "Trade Show" => 15,
            "Email Campaign" => 10,
            "Cold Call" => 5,
            _ => 10
        };

        // Status factor (0-30 points)
        score += lead.Status switch
        {
            "Qualified" => 30,
            "Contacted" => 20,
            "New" => 10,
            "Converted" => 30,
            "Lost" => 0,
            "Unqualified" => 0,
            _ => 5
        };

        lead.LeadScore = Math.Min(score, 100);
        lead.Priority = lead.LeadScore switch
        {
            >= 70 => "High",
            >= 40 => "Medium",
            _ => "Low"
        };
    }
}
