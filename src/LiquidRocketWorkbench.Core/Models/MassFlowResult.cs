namespace LiquidRocketWorkbench.Core.Models;

/// <summary>
/// Geometry-driven propellant flow and optional target-flow comparison.
/// </summary>
public sealed record MassFlowResult(
    double CalculatedMassFlowRateKilogramsPerSecond,
    double OxidizerMassFlowRateKilogramsPerSecond,
    double FuelMassFlowRateKilogramsPerSecond,
    double? TargetMassFlowRateKilogramsPerSecond,
    double? AbsoluteTargetDifferenceKilogramsPerSecond,
    double? RelativeTargetDifference,
    double? PropellantMassConsumedKilograms);
