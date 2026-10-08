using System.ComponentModel.DataAnnotations;
using AcxiomCRM.Validation;

namespace AcxiomCRM.Models.Dtos;

public class LeadDto
{
    public int LeadId { get; set; }
    public string LeadCode { get; set; } = string.Empty;
    public string LeadName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal ExpectedValue { get; set; }
    public int LeadScore { get; set; }
    public string Priority { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
}

public class CreateLeadDto
{
    [Required(ErrorMessage = "Lead Name is required.")]
    [StringLength(150)]
    public string LeadName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [IndianPhone(ErrorMessage = "Enter a valid phone number.")]
    public string Phone { get; set; } = string.Empty;

    [StringLength(200)]
    public string? CompanyName { get; set; }

    [Required]
    public string Source { get; set; } = "Website";

    [Required]
    [ValidLeadStatus]
    public string Status { get; set; } = "New";

    [Range(0, 100000000, ErrorMessage = "Expected Value must be between 0 and 100,000,000.")]
    public decimal ExpectedValue { get; set; }

    public string? AssignedTo { get; set; }
}

public class UpdateLeadDto
{
    [Required(ErrorMessage = "Lead Name is required.")]
    [StringLength(150)]
    public string LeadName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [IndianPhone(ErrorMessage = "Enter a valid phone number.")]
    public string Phone { get; set; } = string.Empty;

    [StringLength(200)]
    public string? CompanyName { get; set; }

    [Required]
    public string Source { get; set; } = string.Empty;

    [Range(0, 100000000, ErrorMessage = "Expected Value must be between 0 and 100,000,000.")]
    public decimal ExpectedValue { get; set; }

    public string? AssignedTo { get; set; }
}

public class ChangeLeadStatusDto
{
    [Required]
    [ValidLeadStatus]
    public string Status { get; set; } = string.Empty;
}

public class ConvertLeadDto
{
    public bool CreateOpportunity { get; set; } = true;
    public string? OpportunityName { get; set; }
    [Range(1, 100000000)]
    public decimal? Amount { get; set; }
}
