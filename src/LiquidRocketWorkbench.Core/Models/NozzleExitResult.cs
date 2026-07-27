namespace LiquidRocketWorkbench.Core.Models;

/// <summary>
/// Ideal static flow state at the nozzle exit.
/// </summary>
public sealed record NozzleExitResult(
    double MachNumber,
    double PressurePascals,
    double TemperatureKelvin,
    double VelocityMetersPerSecond);
