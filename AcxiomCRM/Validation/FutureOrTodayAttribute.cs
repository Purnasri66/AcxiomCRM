using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Validation;

public class FutureOrTodayAttribute : ValidationAttribute
{
    public FutureOrTodayAttribute() : base("The date cannot be earlier than today.") { }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime dateValue)
        {
            if (dateValue.Date < DateTime.Today)
            {
                return new ValidationResult(ErrorMessage ?? "The date cannot be earlier than today.");
            }
        }
        return ValidationResult.Success;
    }
}
