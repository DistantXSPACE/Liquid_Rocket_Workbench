using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.Core.Calculations;

/// <summary>
/// Calculates ideal nozzle static state and velocity using EQ-STATE-01,
/// EQ-STATE-02, and EQ-VELOCITY-01 from <c>docs/references.md</c>.
/// </summary>
public static class NozzleExitCalculator
{
    public static NozzleExitResult Calculate(
        double machNumber,
        double chamberPressurePascals,
        GasProperties gas)
    {
        ArgumentNullException.ThrowIfNull(gas);
        CalculationGuard.RequirePositiveFinite(
            machNumber,
            nameof(machNumber));
        CalculationGuard.RequirePositiveFinite(
            chamberPressurePascals,
            nameof(chamberPressurePascals));
        ValidateGas(gas);

        var gammaDelta = gas.SpecificHeatRatio - 1;
        var logMachEnergyTerm =
            Math.Log(gammaDelta / 2)
            + (2 * Math.Log(machNumber));
        var logStaticDenominator =
            CalculationMath.LogOnePlusFromLog(logMachEnergyTerm);
        var temperatureRatio = Math.Exp(-logStaticDenominator);
        var pressureRatio = Math.Exp(
            -(1 + (1 / gammaDelta)) * logStaticDenominator);
        var exitTemperature =
            gas.ChamberTemperatureKelvin * temperatureRatio;
        var exitPressure = chamberPressurePascals * pressureRatio;
        var logExitVelocity =
            Math.Log(machNumber)
            + (0.5
                * (Math.Log(gas.SpecificHeatRatio)
                    + Math.Log(
                        gas.SpecificGasConstantJoulesPerKilogramKelvin)
                    + Math.Log(exitTemperature)));
        var exitVelocity = Math.Exp(logExitVelocity);

        CalculationGuard.RequirePositiveFiniteResult(
            exitTemperature,
            "Exit temperature");
        CalculationGuard.RequirePositiveFiniteResult(
            exitPressure,
            "Exit pressure");
        CalculationGuard.RequirePositiveFiniteResult(
            exitVelocity,
            "Exit velocity");

        return new NozzleExitResult(
            machNumber,
            exitPressure,
            exitTemperature,
            exitVelocity);
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
