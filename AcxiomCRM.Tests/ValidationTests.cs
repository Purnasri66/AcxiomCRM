using AcxiomCRM.Validation;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace AcxiomCRM.Tests;

public class ValidationTests
{
    [Theory]
    [InlineData("9876543210", true)]
    [InlineData("6123456789", true)]
    [InlineData("7987654321", true)]
    [InlineData("8123456789", true)]
    [InlineData("5123456789", false)] // Invalid starting digit
    [InlineData("987654321", false)]  // 9 digits
    [InlineData("98765432100", false)] // 11 digits
    [InlineData("abcdefghij", false)] // letters
    public void IndianPhoneAttribute_ValidatesCorrectly(string phone, bool expectedValid)
    {
        var attr = new IndianPhoneAttribute();
        var result = attr.GetValidationResult(phone, new ValidationContext(new object()));
        Assert.Equal(expectedValid, result == ValidationResult.Success);
    }

    [Fact]
    public void FutureOrTodayAttribute_RejectsPastDates()
    {
        var attr = new FutureOrTodayAttribute();
        var pastDate = DateTime.Today.AddDays(-1);
        var result = attr.GetValidationResult(pastDate, new ValidationContext(new object()));
        Assert.NotEqual(ValidationResult.Success, result);
    }

    [Fact]
    public void FutureOrTodayAttribute_AcceptsTodayAndFuture()
    {
        var attr = new FutureOrTodayAttribute();
        var todayResult = attr.GetValidationResult(DateTime.Today, new ValidationContext(new object()));
        var futureResult = attr.GetValidationResult(DateTime.Today.AddDays(5), new ValidationContext(new object()));
        Assert.Equal(ValidationResult.Success, todayResult);
        Assert.Equal(ValidationResult.Success, futureResult);
    }

    [Theory]
    [InlineData("New", true)]
    [InlineData("Contacted", true)]
    [InlineData("Qualified", true)]
    [InlineData("Converted", true)]
    [InlineData("Lost", true)]
    [InlineData("Unqualified", true)]
    [InlineData("RandomStatus", false)]
    public void ValidLeadStatusAttribute_ValidatesCorrectly(string status, bool expectedValid)
    {
        var attr = new ValidLeadStatusAttribute();
        var result = attr.GetValidationResult(status, new ValidationContext(new object()));
        Assert.Equal(expectedValid, result == ValidationResult.Success);
    }

    [Theory]
    [InlineData("Call", true)]
    [InlineData("Meeting", true)]
    [InlineData("Email", true)]
    [InlineData("Task", true)]
    [InlineData("InvalidType", false)]
    public void ValidActivityTypeAttribute_ValidatesCorrectly(string type, bool expectedValid)
    {
        var attr = new ValidActivityTypeAttribute();
        var result = attr.GetValidationResult(type, new ValidationContext(new object()));
        Assert.Equal(expectedValid, result == ValidationResult.Success);
    }
}
