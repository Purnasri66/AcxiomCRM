using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AcxiomCRM.Validation;

public class IndianPhoneAttribute : ValidationAttribute
{
    private static readonly Regex PhoneRegex = new(@"^[6-9]\d{9}$", RegexOptions.Compiled);

    public IndianPhoneAttribute() : base("Enter a valid phone number.") { }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string phone && !string.IsNullOrWhiteSpace(phone))
        {
            if (!PhoneRegex.IsMatch(phone))
            {
                return new ValidationResult(ErrorMessage ?? "Enter a valid phone number.");
            }
        }
        return ValidationResult.Success;
    }
}
