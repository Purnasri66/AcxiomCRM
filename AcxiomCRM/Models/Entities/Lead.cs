using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AcxiomCRM.Models.Entities;

public class Lead
{
    [Key]
    public int LeadId { get; set; }

    [Required]
    [StringLength(20)]
    public string LeadCode { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string LeadName { get; set; } = string.Empty;

    [Required]
    [StringLength(256)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(15)]
    public string Phone { get; set; } = string.Empty;

    [StringLength(200)]
    public string? CompanyName { get; set; }

    [Required]
    [StringLength(50)]
    public string Source { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "New";

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 100000000)]
    public decimal ExpectedValue { get; set; }

    public int LeadScore { get; set; }

    [StringLength(50)]
    public string Priority { get; set; } = "Medium";

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedDate { get; set; }

    [Required]
    public string AssignedTo { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; } = false;

    public int? ConvertedCustomerId { get; set; }

    // Foreign keys
    [ForeignKey("AssignedTo")]
    public virtual ApplicationUser? AssignedUser { get; set; }

    [ForeignKey("ConvertedCustomerId")]
    public virtual Customer? ConvertedCustomer { get; set; }

    // Navigation
    public virtual ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
    public virtual ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();
    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();
}
