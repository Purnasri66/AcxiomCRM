using System.ComponentModel.DataAnnotations;
using AcxiomCRM.Models.Entities;
using AcxiomCRM.Validation;

namespace AcxiomCRM.Models.ViewModels;

public class ActivityListViewModel
{
    public IEnumerable<ActivityItemViewModel> Activities { get; set; } = new List<ActivityItemViewModel>();
    public string? TypeFilter { get; set; }
    public string? SortBy { get; set; }
    public bool Ascending { get; set; } = true;
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}

public class ActivityItemViewModel
{
    public int ActivityId { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ActivityDate { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? LeadId { get; set; }
    public string? LeadName { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
    public string AssignedUserName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class ActivityCreateEditViewModel
{
    public int ActivityId { get; set; }

    [Required(ErrorMessage = "Activity type is required.")]
    [ValidActivityType]
    public string ActivityType { get; set; } = "Call";

    [Required(ErrorMessage = "Subject is required.")]
    [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters.")]
    public string Subject { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Activity Date is required.")]
    public DateTime ActivityDate { get; set; } = DateTime.UtcNow;

    public int? CustomerId { get; set; }
    public int? LeadId { get; set; }

    public string? AssignedTo { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Planned";
}
