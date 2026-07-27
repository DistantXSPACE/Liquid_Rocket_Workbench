namespace LiquidRocketWorkbench.Core.Diagnostics;

/// <summary>
/// Immutable result of validating one engine operating point.
/// </summary>
public sealed class ValidationResult
{
    private readonly IReadOnlyList<ValidationIssue> _issues;

    public ValidationResult(IEnumerable<ValidationIssue> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        _issues = Array.AsReadOnly(issues.ToArray());
        IsValid = !_issues.Any(
            static issue => issue.Severity == ValidationSeverity.Error);
        HasWarnings = _issues.Any(
            static issue => issue.Severity == ValidationSeverity.Warning);
    }

    public IReadOnlyList<ValidationIssue> Issues => _issues;

    public bool IsValid { get; }

    public bool HasWarnings { get; }
}
