namespace LiquidRocketWorkbench.Core.Models;

/// <summary>
/// Relationship between nozzle exit pressure and the selected ambient pressure.
/// </summary>
public enum NozzleExpansionState
{
    Underexpanded,
    IdeallyExpanded,
    Overexpanded,
}
