namespace LiquidRocketWorkbench.Core.Models;

/// <summary>
/// Oxidizer and fuel components of a total bipropellant mass flow.
/// </summary>
public sealed record PropellantFlowSplit(
    double OxidizerMassFlowRateKilogramsPerSecond,
    double FuelMassFlowRateKilogramsPerSecond)
{
    public double TotalMassFlowRateKilogramsPerSecond =>
        OxidizerMassFlowRateKilogramsPerSecond
        + FuelMassFlowRateKilogramsPerSecond;
}
