using System.ComponentModel.DataAnnotations;
using AcxiomCRM.Validation;

namespace AcxiomCRM.Models.Dtos;

public class OpportunityDto
{
    public int OpportunityId { get; set; }
    public string OpportunityName { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? LeadId { get; set; }
    public decimal Amount { get; set; }
    public string Stage { get; set; } = string.Empty;
    public int Probability { get; set; }
    public decimal WeightedValue => Amount * Probability / 100;
    public DateTime? ExpectedCloseDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int DealHealthScore { get; set; }
    public DateTime CreatedDate { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
}

public class CreateOpportunityDto
{
    [Required(ErrorMessage = "Opportunity Name is required.")]
    [StringLength(200)]
    public string OpportunityName { get; set; } = string.Empty;

    [Required]
    public int CustomerId { get; set; }

    public int? LeadId { get; set; }

    [Required]
    [Range(0.01, 100000000, ErrorMessage = "Opportunity Amount must be greater than 0.")]
    public decimal Amount { get; set; }

    [Required]
    public string Stage { get; set; } = "Qualification";

    [Range(0, 100, ErrorMessage = "Probability must be between 0 and 100.")]
    public int Probability { get; set; } = 20;

    [FutureOrToday(ErrorMessage = "Expected Close Date cannot be in the past.")]
    public DateTime? ExpectedCloseDate { get; set; }

    public string Status { get; set; } = "Open";
    public string? AssignedTo { get; set; }
}

public class UpdateOpportunityDto
{
    [Required(ErrorMessage = "Opportunity Name is required.")]
    [StringLength(200)]
    public string OpportunityName { get; set; } = string.Empty;

    [Required]
    public int CustomerId { get; set; }

    [Required]
    [Range(0.01, 100000000, ErrorMessage = "Opportunity Amount must be greater than 0.")]
    public decimal Amount { get; set; }

    [Required]
    public string Stage { get; set; } = "Qualification";

    [Range(0, 100, ErrorMessage = "Probability must be between 0 and 100.")]
    public int Probability { get; set; } = 20;

    public DateTime? ExpectedCloseDate { get; set; }
    public string Status { get; set; } = "Open";
    public string? AssignedTo { get; set; }
}

public class ChangeStageDto
{
    [Required]
    public string Stage { get; set; } = string.Empty;
}
