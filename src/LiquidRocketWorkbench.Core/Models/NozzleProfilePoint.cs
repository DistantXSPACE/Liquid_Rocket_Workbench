namespace LiquidRocketWorkbench.Core.Models;

/// <summary>
/// Ideal static flow state at one normalized chamber-to-exit position.
/// </summary>
public sealed record NozzleProfilePoint(
    double NormalizedAxialPosition,
    double MachNumber,
    double PressurePascals,
    double TemperatureKelvin);
