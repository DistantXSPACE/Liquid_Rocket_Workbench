namespace LiquidRocketWorkbench.App.ViewModels;

/// <summary>
/// One display-only step in the application shell workflow.
/// </summary>
public sealed record WorkflowStepViewModel(
    string StepNumber,
    string Title,
    string Description,
    bool IsCurrent);
