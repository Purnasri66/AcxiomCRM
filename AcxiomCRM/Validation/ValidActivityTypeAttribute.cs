using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Validation;

public class ValidActivityTypeAttribute : ValidationAttribute
{
    private static readonly HashSet<string> ValidTypes = new() { "Call", "Meeting", "Email", "Task" };

    public ValidActivityTypeAttribute() : base("Invalid activity type.") { }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string type && !ValidTypes.Contains(type))
        {
            return new ValidationResult($"'{type}' is not a valid activity type. Valid types: Call, Meeting, Email, Task.");
        }
        return ValidationResult.Success;
    }
}
