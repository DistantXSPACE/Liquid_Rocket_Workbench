using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.Core.Calculations;

/// <summary>
/// Calculates geometry-driven ideal choked mass flow using EQ-FLOW-01 from
/// <c>docs/references.md</c>.
/// </summary>
public static class ChokedMassFlowCalculator
{
    public static double Calculate(
        double chamberPressurePascals,
        double throatAreaSquareMeters,
        GasProperties gas)
    {
        ArgumentNullException.ThrowIfNull(gas);
        CalculationGuard.RequirePositiveFinite(
            chamberPressurePascals,
            nameof(chamberPressurePascals));
        CalculationGuard.RequirePositiveFinite(
            throatAreaSquareMeters,
            nameof(throatAreaSquareMeters));
        ValidateGas(gas);

        var gammaDelta = gas.SpecificHeatRatio - 1;
        var exponent = 0.5 + (1 / gammaDelta);
        var logChokingFactor =
            0.5
            * (Math.Log(gas.SpecificHeatRatio)
                - Math.Log(
                    gas.SpecificGasConstantJoulesPerKilogramKelvin))
            - (exponent
                * CalculationMath.LogOnePlus(gammaDelta / 2));
        var logMassFlowRate =
            Math.Log(throatAreaSquareMeters)
            + Math.Log(chamberPressurePascals)
            - (0.5 * Math.Log(gas.ChamberTemperatureKelvin))
            + logChokingFactor;
        var massFlowRate = Math.Exp(logMassFlowRate);

        CalculationGuard.RequirePositiveFiniteResult(
            massFlowRate,
            "Choked mass flow");
        return massFlowRate;
    }

    private static void ValidateGas(GasProperties gas)
    {
        CalculationGuard.RequirePositiveFinite(
            gas.ChamberTemperatureKelvin,
            nameof(gas.ChamberTemperatureKelvin));
        CalculationGuard.RequireGreaterThanOneFinite(
            gas.SpecificHeatRatio,
            nameof(gas.SpecificHeatRatio));
        CalculationGuard.RequirePositiveFinite(
            gas.SpecificGasConstantJoulesPerKilogramKelvin,
            nameof(gas.SpecificGasConstantJoulesPerKilogramKelvin));
    }
}
