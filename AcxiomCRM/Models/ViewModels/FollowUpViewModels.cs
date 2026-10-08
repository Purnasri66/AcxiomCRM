using System.ComponentModel.DataAnnotations;
using AcxiomCRM.Models.Entities;
using AcxiomCRM.Validation;

namespace AcxiomCRM.Models.ViewModels;

public class FollowUpListViewModel
{
    public IEnumerable<FollowUpItemViewModel> FollowUps { get; set; } = new List<FollowUpItemViewModel>();
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

public class FollowUpItemViewModel
{
    public int FollowUpId { get; set; }
    public DateTime FollowUpDate { get; set; }
    public string FollowUpType { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? LeadId { get; set; }
    public string? LeadName { get; set; }
    public int? OpportunityId { get; set; }
    public string? OpportunityName { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
    public string AssignedUserName { get; set; } = string.Empty;
    public bool IsOverdue => Status == "Planned" && FollowUpDate < DateTime.UtcNow;
}

public class FollowUpCreateEditViewModel
{
    public int FollowUpId { get; set; }
    public int? CustomerId { get; set; }
    public int? LeadId { get; set; }
    public int? OpportunityId { get; set; }

    [Required(ErrorMessage = "Follow-up date is required.")]
    [FutureOrToday(ErrorMessage = "Follow-up date cannot be earlier than today.")]
    public DateTime FollowUpDate { get; set; } = DateTime.Today.AddDays(1);

    [Required(ErrorMessage = "Follow-up type is required.")]
    [StringLength(50)]
    public string FollowUpType { get; set; } = "Call";

    [StringLength(1000, ErrorMessage = "Remarks cannot exceed 1000 characters.")]
    public string? Remarks { get; set; }

    [Required]
    public string Status { get; set; } = "Planned";

    public string? AssignedTo { get; set; }
}

public class FollowUpRescheduleViewModel
{
    public int FollowUpId { get; set; }

    [Required(ErrorMessage = "New follow-up date is required.")]
    [FutureOrToday(ErrorMessage = "Follow-up date cannot be earlier than today.")]
    public DateTime NewDate { get; set; } = DateTime.Today.AddDays(1);
}

public class FollowUpCompleteViewModel
{
    public int FollowUpId { get; set; }

    [StringLength(1000)]
    public string? Remarks { get; set; }
}
