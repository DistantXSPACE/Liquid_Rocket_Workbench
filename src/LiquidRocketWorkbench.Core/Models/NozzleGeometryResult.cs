namespace LiquidRocketWorkbench.Core.Models;

/// <summary>
/// Derived circular-nozzle geometry.
/// </summary>
public sealed record NozzleGeometryResult(
    double ThroatAreaSquareMeters,
    double ExitAreaSquareMeters,
    double ExpansionRatio);
