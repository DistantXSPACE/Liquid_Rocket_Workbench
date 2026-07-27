using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.Core.Calculations;

/// <summary>
/// Builds the geometry-driven ideal mass-flow result using EQ-FLOW-01,
/// EQ-MIX-02, EQ-MIX-03, and EQ-MASS-01 from
/// <c>docs/references.md</c>.
/// </summary>
public static class MassFlowCalculator
{
    public static MassFlowResult Calculate(
        double chamberPressurePascals,
        double throatAreaSquareMeters,
        GasProperties gas,
        double mixtureRatio,
        double? targetMassFlowRateKilogramsPerSecond = null,
        double? burnDurationSeconds = null)
    {
        CalculationGuard.RequirePositiveFinite(
            mixtureRatio,
            nameof(mixtureRatio));
        ValidateOptionalPositive(
            targetMassFlowRateKilogramsPerSecond,
            nameof(targetMassFlowRateKilogramsPerSecond));
        ValidateOptionalPositive(
            burnDurationSeconds,
            nameof(burnDurationSeconds));

        var calculatedMassFlowRate = ChokedMassFlowCalculator.Calculate(
            chamberPressurePascals,
            throatAreaSquareMeters,
            gas);
        var split = PropellantFlowCalculator.Split(
            calculatedMassFlowRate,
            mixtureRatio);

        double? absoluteTargetDifference = null;
        double? relativeTargetDifference = null;

        if (targetMassFlowRateKilogramsPerSecond is double targetMassFlowRate)
        {
            absoluteTargetDifference = Math.Abs(
                targetMassFlowRate - calculatedMassFlowRate);
            relativeTargetDifference =
                absoluteTargetDifference / calculatedMassFlowRate;
            CalculationGuard.RequireFiniteResult(
                relativeTargetDifference.Value,
                "Relative target mass-flow difference");
        }

        double? propellantMassConsumed = null;

        if (burnDurationSeconds is double burnDuration)
        {
            propellantMassConsumed = calculatedMassFlowRate * burnDuration;
            CalculationGuard.RequirePositiveFiniteResult(
                propellantMassConsumed.Value,
                "Propellant mass consumption");
        }

        return new MassFlowResult(
            calculatedMassFlowRate,
            split.OxidizerMassFlowRateKilogramsPerSecond,
            split.FuelMassFlowRateKilogramsPerSecond,
            targetMassFlowRateKilogramsPerSecond,
            absoluteTargetDifference,
            relativeTargetDifference,
            propellantMassConsumed);
    }

    private static void ValidateOptionalPositive(
        double? value,
        string parameterName)
    {
        if (value is double presentValue)
        {
            CalculationGuard.RequirePositiveFinite(
                presentValue,
                parameterName);
        }
    }
}
