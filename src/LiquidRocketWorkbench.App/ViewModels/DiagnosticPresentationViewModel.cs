using LiquidRocketWorkbench.Core.Diagnostics;
using LiquidRocketWorkbench.Core.Validation;

namespace LiquidRocketWorkbench.App.ViewModels;

/// <summary>
/// Preserves one Core diagnostic while adding UI-oriented labels.
/// </summary>
public sealed class DiagnosticPresentationViewModel
{
    public DiagnosticPresentationViewModel(ValidationIssue issue)
    {
        ArgumentNullException.ThrowIfNull(issue);

        Code = issue.Code;
        Severity = issue.Severity;
        Message = issue.Message;
        Field = issue.Field;
    }

    public string Code { get; }

    public ValidationSeverity Severity { get; }

    public string SeverityLabel => Severity.ToString();

    public string Message { get; }

    public string? Field { get; }

    public string? FieldLabel =>
        Field switch
        {
            EngineInputFields.TargetMassFlowRate => "Target mass flow",
            null => null,
            _ => Field,
        };

    public bool HasField => Field is not null;

    public bool IsInformation =>
        Severity == ValidationSeverity.Information;

    public bool IsWarning =>
        Severity == ValidationSeverity.Warning;

    public bool IsError =>
        Severity == ValidationSeverity.Error;
}
