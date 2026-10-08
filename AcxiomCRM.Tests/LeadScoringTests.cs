using AcxiomCRM.Models.Entities;
using AcxiomCRM.Services;
using Xunit;

namespace AcxiomCRM.Tests;

public class LeadScoringTests
{
    [Fact]
    public void CalculateScore_ComputesHighValueReferralLeadAsHighPriority()
    {
        var service = new LeadScoringService();
        var lead = new Lead
        {
            ExpectedValue = 5000000, // 40 pts
            Source = "Referral",     // 30 pts
            Status = "Qualified"     // 30 pts
        };

        service.CalculateScore(lead);

        Assert.Equal(100, lead.LeadScore);
        Assert.Equal("High", lead.Priority);
    }

    [Fact]
    public void CalculateScore_ComputesLowValueColdCallLeadAsLowPriority()
    {
        var service = new LeadScoringService();
        var lead = new Lead
        {
            ExpectedValue = 50000,   // 5 pts
            Source = "Cold Call",    // 5 pts
            Status = "New"           // 10 pts
        };

        service.CalculateScore(lead);

        Assert.Equal(20, lead.LeadScore);
        Assert.Equal("Low", lead.Priority);
    }
}
