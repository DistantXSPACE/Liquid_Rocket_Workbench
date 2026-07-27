using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.Core.Calculations;

/// <summary>
/// Samples ideal ambient thrust through the documented standard atmosphere.
/// </summary>
public static class StandardAtmosphereThrustProfileCalculator
{
    public const double AltitudeIncrementMeters = 1_000;

    public const int ProfilePointCount = 51;

    public static StandardAtmosphereThrustProfileResult Calculate(
        double selectedAmbientPressurePascals,
        double chamberPressurePascals,
        double massFlowRateKilogramsPerSecond,
        NozzleGeometryResult geometry,
        NozzleExitResult nozzleExit)
    {
        CalculationGuard.RequireNonnegativeFinite(
            selectedAmbientPressurePascals,
            nameof(selectedAmbientPressurePascals));

        var points = new List<StandardAtmosphereThrustPoint>(
            ProfilePointCount);
        for (var index = 0; index < ProfilePointCount; index++)
        {
            var altitudeMeters = index * AltitudeIncrementMeters;
            var ambientPressure =
                StandardAtmosphere1976Calculator.CalculatePressurePascals(
                    altitudeMeters);
            var performance = AmbientPerformanceCalculator.Calculate(
                ambientPressure,
                chamberPressurePascals,
                massFlowRateKilogramsPerSecond,
                geometry,
                nozzleExit);
            points.Add(
                new StandardAtmosphereThrustPoint(
                    altitudeMeters,
                    ambientPressure,
                    performance.TotalThrustNewtons,
                    performance.NozzleExpansionState));
        }

        double? selectedEquivalentAltitude = null;
        if (StandardAtmosphere1976Calculator
            .TryFindGeopotentialAltitudeMeters(
                selectedAmbientPressurePascals,
                out var equivalentAltitude))
        {
            selectedEquivalentAltitude = equivalentAltitude;
        }

        return new StandardAtmosphereThrustProfileResult(
            points,
            selectedEquivalentAltitude);
    }
}
