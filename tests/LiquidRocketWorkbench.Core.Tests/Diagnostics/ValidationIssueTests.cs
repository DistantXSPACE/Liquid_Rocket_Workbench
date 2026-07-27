using LiquidRocketWorkbench.Core.Diagnostics;

namespace LiquidRocketWorkbench.Core.Tests.Diagnostics;

public sealed class ValidationIssueTests
{
    [Fact]
    public void Constructor_PreservesStableDiagnosticData()
    {
        var issue = new ValidationIssue(
            "INPUT.GAMMA.OUT_OF_RANGE",
            ValidationSeverity.Error,
            "Specific heat ratio must be greater than one.",
            "gas.specificHeatRatio");

        Assert.Equal("INPUT.GAMMA.OUT_OF_RANGE", issue.Code);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal(
            "Specific heat ratio must be greater than one.",
            issue.Message);
        Assert.Equal("gas.specificHeatRatio", issue.Field);
    }

    [Fact]
    public void Constructor_AllowsCalculationWideIssue()
    {
        var issue = new ValidationIssue(
            "MODEL.IDEAL_FLOW_LIMIT",
            ValidationSeverity.Warning,
            "The ideal model does not predict separated flow.");

        Assert.Null(issue.Field);
    }
}
