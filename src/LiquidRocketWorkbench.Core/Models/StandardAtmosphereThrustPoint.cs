namespace LiquidRocketWorkbench.Core.Models;

/// <summary>
/// Ideal thrust at one U.S. Standard Atmosphere geopotential altitude.
/// </summary>
public sealed record StandardAtmosphereThrustPoint(
    double GeopotentialAltitudeMeters,
    double AmbientPressurePascals,
    double TotalThrustNewtons,
    NozzleExpansionState NozzleExpansionState);
