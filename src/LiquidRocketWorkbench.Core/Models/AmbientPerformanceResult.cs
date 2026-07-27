namespace LiquidRocketWorkbench.Core.Models;

/// <summary>
/// Ideal engine performance evaluated at one ambient pressure.
/// </summary>
public sealed record AmbientPerformanceResult(
    double AmbientPressurePascals,
    double MomentumThrustNewtons,
    double PressureThrustNewtons,
    double TotalThrustNewtons,
    double SpecificImpulseSeconds,
    double ThrustCoefficient,
    NozzleExpansionState NozzleExpansionState);
