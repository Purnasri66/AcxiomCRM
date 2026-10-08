using System.ComponentModel.DataAnnotations;
using AcxiomCRM.Validation;

namespace AcxiomCRM.Models.Dtos;

public class ActivityDto
{
    public int ActivityId { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ActivityDate { get; set; }
    public int? CustomerId { get; set; }
    public int? LeadId { get; set; }
    public string AssignedTo { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

public class CreateActivityDto
{
    [Required]
    [ValidActivityType]
    public string ActivityType { get; set; } = "Call";

    [Required]
    [StringLength(200)]
    public string Subject { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    public DateTime ActivityDate { get; set; }

    public int? CustomerId { get; set; }
    public int? LeadId { get; set; }
    public string? AssignedTo { get; set; }
    public string Status { get; set; } = "Planned";
}
