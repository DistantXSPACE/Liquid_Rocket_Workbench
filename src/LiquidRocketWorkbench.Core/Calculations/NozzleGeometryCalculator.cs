using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.Core.Calculations;

/// <summary>
/// Calculates circular-nozzle areas and expansion ratio using EQ-GEO-01 and
/// EQ-GEO-02 from <c>docs/references.md</c>.
/// </summary>
public static class NozzleGeometryCalculator
{
    public static NozzleGeometryResult Calculate(NozzleGeometry nozzle)
    {
        ArgumentNullException.ThrowIfNull(nozzle);
        CalculationGuard.RequirePositiveFinite(
            nozzle.ThroatDiameterMeters,
            nameof(nozzle.ThroatDiameterMeters));
        CalculationGuard.RequirePositiveFinite(
            nozzle.ExitDiameterMeters,
            nameof(nozzle.ExitDiameterMeters));

        if (nozzle.ExitDiameterMeters < nozzle.ThroatDiameterMeters)
        {
            throw new ArgumentException(
                "Exit diameter must be greater than or equal to throat "
                    + "diameter.",
                nameof(nozzle));
        }

        var throatArea = CalculateCircularArea(nozzle.ThroatDiameterMeters);
        var exitArea = CalculateCircularArea(nozzle.ExitDiameterMeters);
        CalculationGuard.RequirePositiveFiniteResult(
            throatArea,
            "Nozzle throat area");
        CalculationGuard.RequirePositiveFiniteResult(
            exitArea,
            "Nozzle exit area");

        var expansionRatio = exitArea / throatArea;
        CalculationGuard.RequirePositiveFiniteResult(
            expansionRatio,
            "Nozzle expansion ratio");
        return new NozzleGeometryResult(
            throatArea,
            exitArea,
            expansionRatio);
    }

    private static double CalculateCircularArea(double diameterMeters)
    {
        var radiusMeters = diameterMeters / 2;
        return Math.PI * radiusMeters * radiusMeters;
    }
}
