namespace LiquidRocketWorkbench.Core.Calculations;

/// <summary>
/// Shared numerical tolerances used by Core calculations.
/// </summary>
public static class CalculationTolerances
{
    public const double AreaRatioRelative = 1e-8;
    public const double NozzlePressureRelative = 0.02;
    public const double TargetMassFlowRelative = 0.05;
}
