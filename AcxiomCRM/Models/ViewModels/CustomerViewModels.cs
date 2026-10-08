using System.ComponentModel.DataAnnotations;
using AcxiomCRM.Models.Entities;
using AcxiomCRM.Validation;

namespace AcxiomCRM.Models.ViewModels;

public class CustomerListViewModel
{
    public IEnumerable<CustomerItemViewModel> Customers { get; set; } = new List<CustomerItemViewModel>();
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public bool Ascending { get; set; } = true;
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}

public class CustomerItemViewModel
{
    public int CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
}

public class CustomerCreateEditViewModel
{
    public int CustomerId { get; set; }

    [Required(ErrorMessage = "Customer Name is required.")]
    [StringLength(150, ErrorMessage = "Customer Name cannot exceed 150 characters.")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [IndianPhone(ErrorMessage = "Enter a valid phone number.")]
    public string Phone { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Company Name cannot exceed 200 characters.")]
    public string? CompanyName { get; set; }

    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
    public string? Address { get; set; }

    [StringLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
    public string? City { get; set; }

    [StringLength(100, ErrorMessage = "State cannot exceed 100 characters.")]
    public string? State { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    [StringLength(50)]
    public string Status { get; set; } = "Active";
}

public class CustomerDetailsViewModel
{
    public Customer Customer { get; set; } = null!;
    public List<Lead> RelatedLeads { get; set; } = new();
    public List<Opportunity> RelatedOpportunities { get; set; } = new();
    public List<FollowUp> RelatedFollowUps { get; set; } = new();
    public List<Activity> RelatedActivities { get; set; } = new();
    public List<CustomerTimelineItemViewModel> UnifiedTimeline { get; set; } = new();
    public List<AuditLog> AuditLogs { get; set; } = new();
}

public class CustomerTimelineItemViewModel
{
    public DateTime Date { get; set; }
    public string ItemType { get; set; } = string.Empty; // Activity, FollowUp, AuditLog
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BadgeClass { get; set; } = "primary";
    public string Icon { get; set; } = "bi-circle";
    public string? UserName { get; set; }
    public string? RecordId { get; set; }
}
