using System.Globalization;
using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.App.ViewModels;

/// <summary>
/// Presentation projection for ideal thrust through the standard atmosphere.
/// </summary>
public sealed class ThrustAltitudeViewModel
{
    private const double PlotLeft = 80;
    private const double PlotRight = 865;
    private const double PlotTop = 22;
    private const double PlotBottom = 188;
    private const double MaximumAltitudeMeters = 50_000;

    private readonly IReadOnlyList<NozzleProfileChartPointViewModel>
        _curvePoints;
    private readonly IReadOnlyList<ThrustAltitudeStationViewModel>
        _stations;

    public ThrustAltitudeViewModel(
        EnginePerformanceResult performance,
        CultureInfo? displayCulture = null)
    {
        ArgumentNullException.ThrowIfNull(performance);
        var culture = displayCulture ?? CultureInfo.CurrentCulture;

        var profile = performance.ThrustAltitudeProfile;
        var minimumThrustKilonewtons =
            profile.Points.Min(
                static point => point.TotalThrustNewtons / 1000);
        var vacuumThrustKilonewtons =
            performance.VacuumPerformance.TotalThrustNewtons / 1000;
        var thrustRange =
            vacuumThrustKilonewtons - minimumThrustKilonewtons;
        if (!double.IsFinite(thrustRange) || thrustRange <= 0)
        {
            throw new ArgumentException(
                "Vacuum thrust must exceed the standard-atmosphere curve.",
                nameof(performance));
        }

        _curvePoints = Array.AsReadOnly(
            profile.Points
                .Select(
                    point =>
                    {
                        var thrustKilonewtons =
                            point.TotalThrustNewtons / 1000;
                        return new NozzleProfileChartPointViewModel(
                            X: ToChartX(
                                point.GeopotentialAltitudeMeters),
                            Y: ToChartY(
                                thrustKilonewtons,
                                minimumThrustKilonewtons,
                                thrustRange));
                    })
                .ToArray());
        _stations = Array.AsReadOnly(
            new[] { 0, 10, 20, 30, 40, 50 }
                .Select(
                    index => CreateStation(
                        profile.Points[index],
                        culture))
                .ToArray());

        MaximumThrustText = Format(
            vacuumThrustKilonewtons,
            "N2",
            "kN",
            culture);
        MinimumThrustText = Format(
            minimumThrustKilonewtons,
            "N2",
            "kN",
            culture);
        SeaLevelSummary =
            $"{Format(
                profile.SeaLevel.TotalThrustNewtons / 1000,
                "N3",
                "kN",
                culture)} at "
            + $"{Format(
                profile.SeaLevel.AmbientPressurePascals / 1000,
                "N3",
                "kPa",
                culture)}";
        HighAltitudeSummary =
            $"{Format(
                profile.MaximumAltitude.TotalThrustNewtons / 1000,
                "N3",
                "kN",
                culture)} at "
            + $"{Format(
                profile.MaximumAltitude.AmbientPressurePascals / 1000,
                "N3",
                "kPa",
                culture)}";
        VacuumSummary = Format(
            vacuumThrustKilonewtons,
            "N3",
            "kN",
            culture);
        ThrustGainText =
            $"+{((profile.MaximumAltitude.TotalThrustNewtons
                    / profile.SeaLevel.TotalThrustNewtons)
                - 1).ToString(
                    "P1",
                    culture)} from sea level to 50 km";

        var selectedPressureKilopascals =
            performance.SelectedAmbientPerformance.AmbientPressurePascals
            / 1000;
        var selectedThrustKilonewtons =
            performance.SelectedAmbientPerformance.TotalThrustNewtons
            / 1000;
        if (profile.SelectedAmbientEquivalentGeopotentialAltitudeMeters
            is double selectedAltitudeMeters)
        {
            HasSelectedStandardAltitude = true;
            SelectedMarkerLeft = ToChartX(selectedAltitudeMeters) - 6;
            SelectedMarkerTop = ToChartY(
                selectedThrustKilonewtons,
                minimumThrustKilonewtons,
                thrustRange) - 6;
            SelectedAmbientSummary =
                $"{Format(
                    selectedPressureKilopascals,
                    "N3",
                    "kPa",
                    culture)} · "
                + $"{Format(
                    selectedThrustKilonewtons,
                    "N3",
                    "kN",
                    culture)} · "
                + $"standard equivalent "
                + $"{Format(
                    selectedAltitudeMeters / 1000,
                    "N1",
                    "km",
                    culture)}";
        }
        else
        {
            SelectedAmbientSummary =
                $"{Format(
                    selectedPressureKilopascals,
                    "N3",
                    "kPa",
                    culture)} · "
                + $"{Format(
                    selectedThrustKilonewtons,
                    "N3",
                    "kN",
                    culture)} · "
                + "outside the 0–50 km standard-altitude range";
        }

        CurveAccessibilitySummary =
            "Ideal thrust versus U.S. Standard Atmosphere geopotential "
            + "altitude. "
            + string.Join(
                "; ",
                Stations.Select(
                    static station =>
                        $"{station.AltitudeText}: "
                        + $"{station.AmbientPressureText}, "
                        + station.ThrustText))
            + $"; vacuum reference {VacuumSummary}.";
    }

    public IReadOnlyList<NozzleProfileChartPointViewModel>
        CurvePoints => _curvePoints;

    public IReadOnlyList<ThrustAltitudeStationViewModel>
        Stations => _stations;

    public string MaximumThrustText { get; }

    public string MinimumThrustText { get; }

    public string SeaLevelSummary { get; }

    public string HighAltitudeSummary { get; }

    public string VacuumSummary { get; }

    public string ThrustGainText { get; }

    public bool HasSelectedStandardAltitude { get; }

    public double SelectedMarkerLeft { get; }

    public double SelectedMarkerTop { get; }

    public string SelectedAmbientSummary { get; }

    public string CurveAccessibilitySummary { get; }

    public string ModelNotice =>
        "Geopotential altitude and pressure follow the U.S. Standard "
        + "Atmosphere 1976, not live weather or a trajectory. The ideal nozzle "
        + "exit state is held fixed while ambient pressure changes; shocks, "
        + "separation, and other off-design losses are not predicted.";

    private static ThrustAltitudeStationViewModel CreateStation(
        StandardAtmosphereThrustPoint point,
        CultureInfo culture)
    {
        return new ThrustAltitudeStationViewModel(
            AltitudeText: Format(
                point.GeopotentialAltitudeMeters / 1000,
                "N0",
                "km",
                culture),
            AmbientPressureText: Format(
                point.AmbientPressurePascals / 1000,
                "N3",
                "kPa",
                culture),
            ThrustText: Format(
                point.TotalThrustNewtons / 1000,
                "N3",
                "kN",
                culture));
    }

    private static double ToChartX(double altitudeMeters)
    {
        return PlotLeft
            + ((altitudeMeters / MaximumAltitudeMeters)
                * (PlotRight - PlotLeft));
    }

    private static double ToChartY(
        double thrustKilonewtons,
        double minimumThrustKilonewtons,
        double thrustRange)
    {
        var normalizedThrust =
            (thrustKilonewtons - minimumThrustKilonewtons)
            / thrustRange;
        return PlotBottom
            - (normalizedThrust * (PlotBottom - PlotTop));
    }

    private static string Format(
        double value,
        string numberFormat,
        string unit,
        CultureInfo culture)
    {
        return $"{value.ToString(
            numberFormat,
            culture)} {unit}";
    }
}
