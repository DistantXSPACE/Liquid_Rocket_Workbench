namespace LiquidRocketWorkbench.Core.Calculations;

/// <summary>
/// Evaluates the lower U.S. Standard Atmosphere 1976 pressure equations
/// EQ-ATM-01/02 from <c>docs/references.md</c>.
/// </summary>
public static class StandardAtmosphere1976Calculator
{
    public const double MaximumSupportedGeopotentialAltitudeMeters = 50_000;

    private const double StandardMolarMassKilogramsPerMole = 0.0289644;
    private const double MolarGasConstantJoulesPerMoleKelvin = 8.31432;

    private static readonly IReadOnlyList<AtmosphereLayer> Layers =
        Array.AsReadOnly(
            [
                new AtmosphereLayer(
                    BaseAltitudeMeters: 0,
                    BaseTemperatureKelvin: 288.15,
                    BasePressurePascals: 101_325,
                    TemperatureLapseRateKelvinPerMeter: -0.0065),
                new AtmosphereLayer(
                    BaseAltitudeMeters: 11_000,
                    BaseTemperatureKelvin: 216.65,
                    BasePressurePascals: 22_632.0639734629,
                    TemperatureLapseRateKelvinPerMeter: 0),
                new AtmosphereLayer(
                    BaseAltitudeMeters: 20_000,
                    BaseTemperatureKelvin: 216.65,
                    BasePressurePascals: 5_474.88866967778,
                    TemperatureLapseRateKelvinPerMeter: 0.001),
                new AtmosphereLayer(
                    BaseAltitudeMeters: 32_000,
                    BaseTemperatureKelvin: 228.65,
                    BasePressurePascals: 868.018684755228,
                    TemperatureLapseRateKelvinPerMeter: 0.0028),
                new AtmosphereLayer(
                    BaseAltitudeMeters: 47_000,
                    BaseTemperatureKelvin: 270.65,
                    BasePressurePascals: 110.906305554966,
                    TemperatureLapseRateKelvinPerMeter: 0),
            ]);

    public static double CalculatePressurePascals(
        double geopotentialAltitudeMeters)
    {
        if (!double.IsFinite(geopotentialAltitudeMeters)
            || geopotentialAltitudeMeters < 0
            || geopotentialAltitudeMeters
                > MaximumSupportedGeopotentialAltitudeMeters)
        {
            throw new ArgumentOutOfRangeException(
                nameof(geopotentialAltitudeMeters),
                geopotentialAltitudeMeters,
                $"Geopotential altitude must be between 0 and "
                    + $"{MaximumSupportedGeopotentialAltitudeMeters:R} "
                    + "meters.");
        }

        var layer = Layers[0];
        foreach (var candidate in Layers)
        {
            if (candidate.BaseAltitudeMeters > geopotentialAltitudeMeters)
            {
                break;
            }

            layer = candidate;
        }

        var altitudeDelta =
            geopotentialAltitudeMeters - layer.BaseAltitudeMeters;
        double pressure;
        if (layer.TemperatureLapseRateKelvinPerMeter == 0)
        {
            pressure =
                layer.BasePressurePascals
                * Math.Exp(
                    -PhysicalConstants
                        .StandardGravityMetersPerSecondSquared
                    * StandardMolarMassKilogramsPerMole
                    * altitudeDelta
                    / MolarGasConstantJoulesPerMoleKelvin
                    / layer.BaseTemperatureKelvin);
        }
        else
        {
            var temperature =
                layer.BaseTemperatureKelvin
                + (layer.TemperatureLapseRateKelvinPerMeter
                    * altitudeDelta);
            var exponent =
                PhysicalConstants.StandardGravityMetersPerSecondSquared
                * StandardMolarMassKilogramsPerMole
                / MolarGasConstantJoulesPerMoleKelvin
                / layer.TemperatureLapseRateKelvinPerMeter;
            pressure =
                layer.BasePressurePascals
                * Math.Pow(
                    layer.BaseTemperatureKelvin / temperature,
                    exponent);
        }

        CalculationGuard.RequirePositiveFiniteResult(
            pressure,
            "Standard-atmosphere pressure");
        return pressure;
    }

    public static bool TryFindGeopotentialAltitudeMeters(
        double pressurePascals,
        out double geopotentialAltitudeMeters)
    {
        CalculationGuard.RequireNonnegativeFinite(
            pressurePascals,
            nameof(pressurePascals));

        var maximumAltitudePressure = CalculatePressurePascals(
            MaximumSupportedGeopotentialAltitudeMeters);
        if (pressurePascals > PhysicalConstants.StandardSeaLevelPressurePascals
            || pressurePascals < maximumAltitudePressure)
        {
            geopotentialAltitudeMeters = double.NaN;
            return false;
        }

        if (pressurePascals
            == PhysicalConstants.StandardSeaLevelPressurePascals)
        {
            geopotentialAltitudeMeters = 0;
            return true;
        }

        if (pressurePascals == maximumAltitudePressure)
        {
            geopotentialAltitudeMeters =
                MaximumSupportedGeopotentialAltitudeMeters;
            return true;
        }

        var lowerAltitude = 0.0;
        var upperAltitude = MaximumSupportedGeopotentialAltitudeMeters;
        for (var iteration = 0; iteration < 100; iteration++)
        {
            var midpointAltitude =
                lowerAltitude + ((upperAltitude - lowerAltitude) / 2);
            var midpointPressure =
                CalculatePressurePascals(midpointAltitude);
            var relativePressureDifference =
                Math.Abs(midpointPressure - pressurePascals)
                / pressurePascals;

            if (relativePressureDifference <= 1e-10)
            {
                geopotentialAltitudeMeters = midpointAltitude;
                return true;
            }

            if (midpointPressure > pressurePascals)
            {
                lowerAltitude = midpointAltitude;
            }
            else
            {
                upperAltitude = midpointAltitude;
            }
        }

        geopotentialAltitudeMeters =
            lowerAltitude + ((upperAltitude - lowerAltitude) / 2);
        return true;
    }

    private sealed record AtmosphereLayer(
        double BaseAltitudeMeters,
        double BaseTemperatureKelvin,
        double BasePressurePascals,
        double TemperatureLapseRateKelvinPerMeter);
}
