using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Validation;

public class ValidLeadStatusAttribute : ValidationAttribute
{
    private static readonly HashSet<string> ValidStatuses = new()
    {
        "New", "Contacted", "Qualified", "Converted", "Lost", "Unqualified"
    };

    public ValidLeadStatusAttribute() : base("Invalid lead status.") { }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string status && !ValidStatuses.Contains(status))
        {
            return new ValidationResult($"'{status}' is not a valid lead status. Valid statuses: {string.Join(", ", ValidStatuses)}");
        }
        return ValidationResult.Success;
    }
}
