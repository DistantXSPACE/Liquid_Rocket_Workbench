using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.Core.Calculations;

/// <summary>
/// Samples EQ-STATE-01/02 along the normalized visualization policy
/// POLICY-PROFILE-01/02 in <c>docs/references.md</c>.
/// </summary>
public static class NozzleFlowProfileCalculator
{
    public const double NormalizedThroatPosition = 0.35;

    public const int ChamberToThroatSegmentCount = 8;

    public const int ThroatToExitSegmentCount = 16;

    public const int ProfilePointCount =
        ChamberToThroatSegmentCount + ThroatToExitSegmentCount + 1;

    public static NozzleFlowProfileResult Calculate(
        double exitMachNumber,
        double chamberPressurePascals,
        GasProperties gas)
    {
        ArgumentNullException.ThrowIfNull(gas);
        CalculationGuard.RequirePositiveFinite(
            exitMachNumber,
            nameof(exitMachNumber));
        if (exitMachNumber < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(exitMachNumber),
                exitMachNumber,
                "Exit Mach number must be at least one.");
        }

        CalculationGuard.RequirePositiveFinite(
            chamberPressurePascals,
            nameof(chamberPressurePascals));
        ValidateGas(gas);

        var points = new List<NozzleProfilePoint>(ProfilePointCount)
        {
            new(
                NormalizedAxialPosition: 0,
                MachNumber: 0,
                PressurePascals: chamberPressurePascals,
                TemperatureKelvin: gas.ChamberTemperatureKelvin),
        };

        for (var index = 1;
             index <= ChamberToThroatSegmentCount;
             index++)
        {
            var segmentPosition =
                (double)index / ChamberToThroatSegmentCount;
            var normalizedPosition =
                NormalizedThroatPosition * segmentPosition;
            var machNumber = SmoothStep(segmentPosition);
            points.Add(
                CalculatePoint(
                    normalizedPosition,
                    machNumber,
                    chamberPressurePascals,
                    gas));
        }

        for (var index = 1;
             index <= ThroatToExitSegmentCount;
             index++)
        {
            var segmentPosition =
                (double)index / ThroatToExitSegmentCount;
            var normalizedPosition =
                NormalizedThroatPosition
                + ((1 - NormalizedThroatPosition) * segmentPosition);
            var machNumber =
                1
                + ((exitMachNumber - 1) * SmoothStep(segmentPosition));
            points.Add(
                CalculatePoint(
                    normalizedPosition,
                    machNumber,
                    chamberPressurePascals,
                    gas));
        }

        return new NozzleFlowProfileResult(
            points,
            ChamberToThroatSegmentCount);
    }

    private static NozzleProfilePoint CalculatePoint(
        double normalizedAxialPosition,
        double machNumber,
        double chamberPressurePascals,
        GasProperties gas)
    {
        var state = NozzleExitCalculator.Calculate(
            machNumber,
            chamberPressurePascals,
            gas);

        return new NozzleProfilePoint(
            normalizedAxialPosition,
            state.MachNumber,
            state.PressurePascals,
            state.TemperatureKelvin);
    }

    private static double SmoothStep(double value)
    {
        return value * value * (3 - (2 * value));
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
