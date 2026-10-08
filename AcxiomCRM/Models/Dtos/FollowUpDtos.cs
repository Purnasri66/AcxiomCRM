using System.ComponentModel.DataAnnotations;
using AcxiomCRM.Validation;

namespace AcxiomCRM.Models.Dtos;

public class FollowUpDto
{
    public int FollowUpId { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? LeadId { get; set; }
    public string? LeadName { get; set; }
    public int? OpportunityId { get; set; }
    public DateTime FollowUpDate { get; set; }
    public string FollowUpType { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public string Status { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public class CreateFollowUpDto
{
    public int? CustomerId { get; set; }
    public int? LeadId { get; set; }
    public int? OpportunityId { get; set; }

    [Required]
    [FutureOrToday(ErrorMessage = "Follow-up date cannot be earlier than today.")]
    public DateTime FollowUpDate { get; set; }

    [Required]
    public string FollowUpType { get; set; } = "Call";

    [StringLength(1000)]
    public string? Remarks { get; set; }

    public string? AssignedTo { get; set; }
}

public class UpdateFollowUpDto
{
    public int? CustomerId { get; set; }
    public int? LeadId { get; set; }
    public int? OpportunityId { get; set; }

    [Required]
    public DateTime FollowUpDate { get; set; }

    [Required]
    public string FollowUpType { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Remarks { get; set; }

    public string? AssignedTo { get; set; }
}

public class RescheduleFollowUpDto
{
    [Required]
    [FutureOrToday(ErrorMessage = "Follow-up date cannot be earlier than today.")]
    public DateTime NewDate { get; set; }
}

public class CompleteFollowUpDto
{
    [StringLength(1000)]
    public string? Remarks { get; set; }
}
