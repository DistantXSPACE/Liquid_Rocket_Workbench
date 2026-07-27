namespace LiquidRocketWorkbench.App.ViewModels;

/// <summary>
/// Mutually exclusive presentation states for one calculation workflow.
/// </summary>
public enum CalculationWorkflowState
{
    Empty,
    Loading,
    Error,
    Success,
}
