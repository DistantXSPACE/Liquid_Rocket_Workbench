using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.Core.Calculations;

/// <summary>
/// Calculates ideal characteristic velocity using EQ-CSTAR-01 and
/// EQ-CSTAR-02 from <c>docs/references.md</c>.
/// </summary>
public static class CharacteristicVelocityCalculator
{
    public static double CalculateIdeal(GasProperties gas)
    {
        ArgumentNullException.ThrowIfNull(gas);
        CalculationGuard.RequirePositiveFinite(
            gas.ChamberTemperatureKelvin,
            nameof(gas.ChamberTemperatureKelvin));
        CalculationGuard.RequireGreaterThanOneFinite(
            gas.SpecificHeatRatio,
            nameof(gas.SpecificHeatRatio));
        CalculationGuard.RequirePositiveFinite(
            gas.SpecificGasConstantJoulesPerKilogramKelvin,
            nameof(gas.SpecificGasConstantJoulesPerKilogramKelvin));

        var gammaDelta = gas.SpecificHeatRatio - 1;
        var exponent = 0.5 + (1 / gammaDelta);
        var logCharacteristicVelocity =
            0.5
            * (Math.Log(
                    gas.SpecificGasConstantJoulesPerKilogramKelvin)
                + Math.Log(gas.ChamberTemperatureKelvin)
                - Math.Log(gas.SpecificHeatRatio))
            + (exponent
                * CalculationMath.LogOnePlus(gammaDelta / 2));
        var characteristicVelocity = Math.Exp(logCharacteristicVelocity);

        CalculationGuard.RequirePositiveFiniteResult(
            characteristicVelocity,
            "Characteristic velocity");
        return characteristicVelocity;
    }

    public static double CalculateFromMassFlow(
        double chamberPressurePascals,
        double throatAreaSquareMeters,
        double massFlowRateKilogramsPerSecond)
    {
        CalculationGuard.RequirePositiveFinite(
            chamberPressurePascals,
            nameof(chamberPressurePascals));
        CalculationGuard.RequirePositiveFinite(
            throatAreaSquareMeters,
            nameof(throatAreaSquareMeters));
        CalculationGuard.RequirePositiveFinite(
            massFlowRateKilogramsPerSecond,
            nameof(massFlowRateKilogramsPerSecond));

        var logCharacteristicVelocity =
            Math.Log(chamberPressurePascals)
            + Math.Log(throatAreaSquareMeters)
            - Math.Log(massFlowRateKilogramsPerSecond);
        var characteristicVelocity = Math.Exp(logCharacteristicVelocity);

        CalculationGuard.RequirePositiveFiniteResult(
            characteristicVelocity,
            "Characteristic velocity");
        return characteristicVelocity;
    }
}
