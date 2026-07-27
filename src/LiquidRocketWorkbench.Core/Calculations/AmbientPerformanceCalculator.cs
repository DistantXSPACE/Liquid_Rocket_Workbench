using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.Core.Calculations;

/// <summary>
/// Calculates thrust, specific impulse, and thrust coefficient using
/// EQ-THRUST-01 through EQ-THRUST-03, EQ-ISP-01, and EQ-CF-01 from
/// <c>docs/references.md</c>, then applies the shared nozzle-state
/// classification policy.
/// </summary>
public static class AmbientPerformanceCalculator
{
    public static AmbientPerformanceResult Calculate(
        double ambientPressurePascals,
        double chamberPressurePascals,
        double massFlowRateKilogramsPerSecond,
        NozzleGeometryResult geometry,
        NozzleExitResult nozzleExit)
    {
        ArgumentNullException.ThrowIfNull(geometry);
        ArgumentNullException.ThrowIfNull(nozzleExit);
        CalculationGuard.RequireNonnegativeFinite(
            ambientPressurePascals,
            nameof(ambientPressurePascals));
        CalculationGuard.RequirePositiveFinite(
            chamberPressurePascals,
            nameof(chamberPressurePascals));
        CalculationGuard.RequirePositiveFinite(
            massFlowRateKilogramsPerSecond,
            nameof(massFlowRateKilogramsPerSecond));
        ValidateGeometry(geometry);
        ValidateNozzleExit(nozzleExit);

        var momentumThrust =
            massFlowRateKilogramsPerSecond
            * nozzleExit.VelocityMetersPerSecond;
        var pressureThrust =
            (nozzleExit.PressurePascals - ambientPressurePascals)
            * geometry.ExitAreaSquareMeters;
        var totalThrust = momentumThrust + pressureThrust;
        var specificImpulse =
            totalThrust
            / massFlowRateKilogramsPerSecond
            / PhysicalConstants.StandardGravityMetersPerSecondSquared;
        var thrustCoefficient =
            totalThrust
            / chamberPressurePascals
            / geometry.ThroatAreaSquareMeters;

        CalculationGuard.RequireFiniteResult(
            momentumThrust,
            "Momentum thrust");
        CalculationGuard.RequireFiniteResult(
            pressureThrust,
            "Pressure thrust");
        CalculationGuard.RequireFiniteResult(totalThrust, "Total thrust");
        CalculationGuard.RequireFiniteResult(
            specificImpulse,
            "Specific impulse");
        CalculationGuard.RequireFiniteResult(
            thrustCoefficient,
            "Thrust coefficient");
        var nozzleExpansionState = NozzleExpansionClassifier.Classify(
            nozzleExit.PressurePascals,
            ambientPressurePascals);

        return new AmbientPerformanceResult(
            ambientPressurePascals,
            momentumThrust,
            pressureThrust,
            totalThrust,
            specificImpulse,
            thrustCoefficient,
            nozzleExpansionState);
    }

    private static void ValidateGeometry(NozzleGeometryResult geometry)
    {
        CalculationGuard.RequirePositiveFinite(
            geometry.ThroatAreaSquareMeters,
            nameof(geometry.ThroatAreaSquareMeters));
        CalculationGuard.RequirePositiveFinite(
            geometry.ExitAreaSquareMeters,
            nameof(geometry.ExitAreaSquareMeters));
        CalculationGuard.RequirePositiveFinite(
            geometry.ExpansionRatio,
            nameof(geometry.ExpansionRatio));

        if (geometry.ExitAreaSquareMeters < geometry.ThroatAreaSquareMeters
            || geometry.ExpansionRatio < 1)
        {
            throw new ArgumentException(
                "Nozzle result must describe an exit area greater than or "
                    + "equal to its throat area.",
                nameof(geometry));
        }

        var calculatedExpansionRatio =
            geometry.ExitAreaSquareMeters
            / geometry.ThroatAreaSquareMeters;
        var relativeExpansionRatioDifference =
            Math.Abs(calculatedExpansionRatio - geometry.ExpansionRatio)
            / Math.Max(calculatedExpansionRatio, geometry.ExpansionRatio);

        if (!double.IsFinite(calculatedExpansionRatio)
            || relativeExpansionRatioDifference
                > CalculationTolerances.AreaRatioRelative)
        {
            throw new ArgumentException(
                "Nozzle expansion ratio must agree with exit area divided by "
                    + "throat area.",
                nameof(geometry));
        }
    }

    private static void ValidateNozzleExit(NozzleExitResult nozzleExit)
    {
        CalculationGuard.RequirePositiveFinite(
            nozzleExit.MachNumber,
            nameof(nozzleExit.MachNumber));
        CalculationGuard.RequireNonnegativeFinite(
            nozzleExit.PressurePascals,
            nameof(nozzleExit.PressurePascals));
        CalculationGuard.RequirePositiveFinite(
            nozzleExit.TemperatureKelvin,
            nameof(nozzleExit.TemperatureKelvin));
        CalculationGuard.RequirePositiveFinite(
            nozzleExit.VelocityMetersPerSecond,
            nameof(nozzleExit.VelocityMetersPerSecond));
    }
}
