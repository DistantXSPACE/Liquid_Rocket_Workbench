namespace LiquidRocketWorkbench.Core.Diagnostics;

/// <summary>
/// Describes a stable, user-facing problem or warning associated with an input
/// field or with the operating point as a whole.
/// </summary>
/// <param name="Code">
/// Stable machine-readable identifier suitable for tests and UI mapping.
/// </param>
/// <param name="Severity">The effect the issue has on calculation.</param>
/// <param name="Message">Actionable user-facing text.</param>
/// <param name="Field">
/// Stable input-field identifier, or <see langword="null"/> for a cross-field
/// or calculation-wide issue.
/// </param>
public sealed record ValidationIssue(
    string Code,
    ValidationSeverity Severity,
    string Message,
    string? Field = null);
