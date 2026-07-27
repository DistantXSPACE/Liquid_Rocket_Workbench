using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.Core.Calculations;

/// <summary>
/// Splits total bipropellant flow using EQ-MIX-01 through EQ-MIX-03 from
/// <c>docs/references.md</c>.
/// </summary>
public static class PropellantFlowCalculator
{
    public static PropellantFlowSplit Split(
        double totalMassFlowRateKilogramsPerSecond,
        double mixtureRatio)
    {
        CalculationGuard.RequirePositiveFinite(
            totalMassFlowRateKilogramsPerSecond,
            nameof(totalMassFlowRateKilogramsPerSecond));
        CalculationGuard.RequirePositiveFinite(
            mixtureRatio,
            nameof(mixtureRatio));

        double oxidizerMassFlowRate;
        double fuelMassFlowRate;

        if (mixtureRatio >= 1)
        {
            fuelMassFlowRate =
                totalMassFlowRateKilogramsPerSecond / (1 + mixtureRatio);
            oxidizerMassFlowRate =
                totalMassFlowRateKilogramsPerSecond - fuelMassFlowRate;
        }
        else
        {
            oxidizerMassFlowRate =
                totalMassFlowRateKilogramsPerSecond
                * mixtureRatio
                / (1 + mixtureRatio);
            fuelMassFlowRate =
                totalMassFlowRateKilogramsPerSecond - oxidizerMassFlowRate;
        }

        if (!double.IsFinite(oxidizerMassFlowRate)
            || !double.IsFinite(fuelMassFlowRate)
            || oxidizerMassFlowRate <= 0
            || fuelMassFlowRate <= 0)
        {
            throw new ArithmeticException(
                "The supplied flow and mixture ratio cannot be represented as "
                    + "two positive finite mass-flow values.");
        }

        return new PropellantFlowSplit(
            oxidizerMassFlowRate,
            fuelMassFlowRate);
    }
}
