using System.ComponentModel.DataAnnotations;
using AcxiomCRM.Models.Entities;
using AcxiomCRM.Validation;

namespace AcxiomCRM.Models.ViewModels;

public class OpportunityListViewModel
{
    public IEnumerable<OpportunityItemViewModel> Opportunities { get; set; } = new List<OpportunityItemViewModel>();
    public string? Search { get; set; }
    public string? StageFilter { get; set; }
    public string? SortBy { get; set; }
    public bool Ascending { get; set; } = true;
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}

public class OpportunityItemViewModel
{
    public int OpportunityId { get; set; }
    public string OpportunityName { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Stage { get; set; } = string.Empty;
    public int Probability { get; set; }
    public decimal WeightedValue => Amount * Probability / 100;
    public DateTime? ExpectedCloseDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int DealHealthScore { get; set; }
    public string DealHealthColor { get; set; } = "success";
    public string DealHealthLabel { get; set; } = "Healthy";
    public string AssignedTo { get; set; } = string.Empty;
    public string AssignedUserName { get; set; } = string.Empty;
}

public class OpportunityCreateEditViewModel
{
    public int OpportunityId { get; set; }

    [Required(ErrorMessage = "Opportunity Name is required.")]
    [StringLength(200, ErrorMessage = "Opportunity Name cannot exceed 200 characters.")]
    public string OpportunityName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Customer is required.")]
    public int CustomerId { get; set; }

    public int? LeadId { get; set; }

    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.01, 100000000, ErrorMessage = "Opportunity Amount must be greater than 0.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Stage is required.")]
    [StringLength(50)]
    public string Stage { get; set; } = "Qualification";

    [Required(ErrorMessage = "Probability is required.")]
    [Range(0, 100, ErrorMessage = "Probability must be between 0 and 100.")]
    public int Probability { get; set; } = 20;

    [FutureOrToday(ErrorMessage = "Expected Close Date cannot be in the past.")]
    public DateTime? ExpectedCloseDate { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    public string Status { get; set; } = "Open";

    public string? AssignedTo { get; set; }
}

public class OpportunityDetailsViewModel
{
    public Opportunity Opportunity { get; set; } = null!;
    public int DealHealthScore { get; set; }
    public string DealHealthColor { get; set; } = "success";
    public string DealHealthLabel { get; set; } = "Healthy";
    public List<FollowUp> RelatedFollowUps { get; set; } = new();
    public List<Activity> RelatedActivities { get; set; } = new();
}

public class KanbanBoardViewModel
{
    public Dictionary<string, List<OpportunityItemViewModel>> Stages { get; set; } = new();
}
