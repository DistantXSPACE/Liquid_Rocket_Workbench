namespace LiquidRocketWorkbench.Core.Models;

/// <summary>
/// Constant ideal-gas properties and chamber total temperature for one
/// operating point.
/// </summary>
public sealed record GasProperties(
    double ChamberTemperatureKelvin,
    double SpecificHeatRatio,
    double SpecificGasConstantJoulesPerKilogramKelvin);
