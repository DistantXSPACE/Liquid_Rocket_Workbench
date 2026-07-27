namespace LiquidRocketWorkbench.Core.Calculations;

/// <summary>
/// Shared thresholds that trigger warnings about ideal-model limitations.
/// </summary>
public static class ModelWarningThresholds
{
    /// <summary>
    /// Conservative exit-to-ambient pressure-ratio warning threshold based on
    /// the upper no-separation guidance value in NASA SP-8041.
    /// </summary>
    public const double MinimumExitToAmbientPressureRatio = 0.4;
}
