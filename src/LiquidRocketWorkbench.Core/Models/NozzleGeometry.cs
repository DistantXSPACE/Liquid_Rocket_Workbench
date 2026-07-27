namespace LiquidRocketWorkbench.Core.Models;

/// <summary>
/// User-supplied circular nozzle dimensions in canonical SI units.
/// </summary>
public sealed record NozzleGeometry(
    double ThroatDiameterMeters,
    double ExitDiameterMeters);
