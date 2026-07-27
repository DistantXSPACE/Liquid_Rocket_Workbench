using LiquidRocketWorkbench.Core.Diagnostics;

namespace LiquidRocketWorkbench.Core.Tests.Diagnostics;

public sealed class ValidationResultTests
{
    [Fact]
    public void Constructor_CopiesIssuesAndReportsWarningWithoutInvalidating()
    {
        var source = new List<ValidationIssue>
        {
            new(
                "MODEL.WARNING",
                ValidationSeverity.Warning,
                "A model warning."),
        };

        var result = new ValidationResult(source);
        source.Add(
            new ValidationIssue(
                "LATE.ERROR",
                ValidationSeverity.Error,
                "A late mutation."));

        Assert.True(result.IsValid);
        Assert.True(result.HasWarnings);
        Assert.Single(result.Issues);
    }

    [Fact]
    public void Constructor_ReportsErrorAsInvalid()
    {
        var result = new ValidationResult(
            [
                new ValidationIssue(
                    "INPUT.ERROR",
                    ValidationSeverity.Error,
                    "An input error."),
            ]);

        Assert.False(result.IsValid);
        Assert.False(result.HasWarnings);
    }
}
