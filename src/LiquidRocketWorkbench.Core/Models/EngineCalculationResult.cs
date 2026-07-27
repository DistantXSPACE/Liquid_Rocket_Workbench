using LiquidRocketWorkbench.Core.Diagnostics;

namespace LiquidRocketWorkbench.Core.Models;

/// <summary>
/// Represents either a complete ideal-engine result or structured issues that
/// prevented calculation.
/// </summary>
public sealed class EngineCalculationResult
{
    private readonly IReadOnlyList<ValidationIssue> _issues;

    private EngineCalculationResult(
        EnginePerformanceResult? performance,
        IEnumerable<ValidationIssue> issues)
    {
        Performance = performance;
        _issues = Array.AsReadOnly(issues.ToArray());
    }

    public bool IsSuccess => Performance is not null;

    public EnginePerformanceResult? Performance { get; }

    public IReadOnlyList<ValidationIssue> Issues => _issues;

    internal static EngineCalculationResult Succeeded(
        EnginePerformanceResult performance)
    {
        ArgumentNullException.ThrowIfNull(performance);
        return new EngineCalculationResult(
            performance,
            performance.Diagnostics);
    }

    internal static EngineCalculationResult Failed(
        IEnumerable<ValidationIssue> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);
        return new EngineCalculationResult(
            performance: null,
            issues);
    }
}
