using System.ComponentModel.DataAnnotations;
using AcxiomCRM.Models.Entities;
using AcxiomCRM.Validation;

namespace AcxiomCRM.Models.ViewModels;

public class LeadListViewModel
{
    public IEnumerable<LeadItemViewModel> Leads { get; set; } = new List<LeadItemViewModel>();
    public string? Search { get; set; }
    public string? StatusFilter { get; set; }
    public string? SortBy { get; set; }
    public bool Ascending { get; set; } = true;
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}

public class LeadItemViewModel
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
    public string Priority { get; set; } = "Medium";
    public DateTime CreatedDate { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
    public string AssignedUserName { get; set; } = string.Empty;
}

public class LeadCreateEditViewModel
{
    public int LeadId { get; set; }

    [Required(ErrorMessage = "Lead Name is required.")]
    [StringLength(150, ErrorMessage = "Lead Name cannot exceed 150 characters.")]
    public string LeadName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [IndianPhone(ErrorMessage = "Enter a valid phone number.")]
    public string Phone { get; set; } = string.Empty;

    [StringLength(200)]
    public string? CompanyName { get; set; }

    [Required(ErrorMessage = "Lead Source is required.")]
    [StringLength(50)]
    public string Source { get; set; } = "Website";

    [Required(ErrorMessage = "Status is required.")]
    [ValidLeadStatus]
    public string Status { get; set; } = "New";

    [Required(ErrorMessage = "Expected Value is required.")]
    [Range(0, 100000000, ErrorMessage = "Expected Value must be between 0 and 100,000,000.")]
    public decimal ExpectedValue { get; set; }

    public string? AssignedTo { get; set; }
}

public class LeadDetailsViewModel
{
    public Lead Lead { get; set; } = null!;
    public IEnumerable<string> ValidNextTransitions { get; set; } = new List<string>();
    public List<Opportunity> RelatedOpportunities { get; set; } = new();
    public List<FollowUp> RelatedFollowUps { get; set; } = new();
    public List<Activity> RelatedActivities { get; set; } = new();
}

public class LeadConvertViewModel
{
    public int LeadId { get; set; }
    public string LeadCode { get; set; } = string.Empty;
    public string LeadName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? CompanyName { get; set; }

    public bool CreateOpportunity { get; set; } = true;

    [StringLength(200)]
    public string? OpportunityName { get; set; }

    [Range(1, 100000000, ErrorMessage = "Opportunity Amount must be greater than 0.")]
    public decimal? Amount { get; set; }
}
