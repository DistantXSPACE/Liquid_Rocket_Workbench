using System.Globalization;
using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.App.ViewModels;

/// <summary>
/// Presentation projection for one normalized nozzle profile series.
/// </summary>
public sealed class NozzleProfileSeriesViewModel
{
    private const double PlotLeft = 80;
    private const double PlotRight = 865;
    private const double PlotTop = 18;
    private const double PlotBottom = 108;

    private readonly IReadOnlyList<NozzleProfileChartPointViewModel>
        _plotPoints;
    private readonly IReadOnlyList<NozzleProfileStationValueViewModel>
        _stations;

    public NozzleProfileSeriesViewModel(
        string title,
        string unit,
        string strokeColor,
        NozzleFlowProfileResult profile,
        Func<NozzleProfilePoint, double> valueSelector,
        string numberFormat,
        CultureInfo? displayCulture = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(unit);
        ArgumentException.ThrowIfNullOrWhiteSpace(strokeColor);
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(valueSelector);
        ArgumentException.ThrowIfNullOrWhiteSpace(numberFormat);
        var culture = displayCulture ?? CultureInfo.CurrentCulture;

        Title = title;
        Unit = unit;
        StrokeColor = strokeColor;

        var values = profile.Points.Select(valueSelector).ToArray();
        var minimumValue = values.Min();
        var maximumValue = values.Max();
        var valueRange = maximumValue - minimumValue;
        if (!double.IsFinite(minimumValue)
            || !double.IsFinite(maximumValue)
            || valueRange <= 0)
        {
            throw new ArgumentException(
                "Profile series values must have a finite nonzero range.",
                nameof(profile));
        }

        _plotPoints = Array.AsReadOnly(
            profile.Points
                .Select(
                    (point, index) =>
                    {
                        var normalizedValue =
                            (values[index] - minimumValue) / valueRange;
                        return new NozzleProfileChartPointViewModel(
                            X: PlotLeft
                                + (point.NormalizedAxialPosition
                                    * (PlotRight - PlotLeft)),
                            Y: PlotBottom
                                - (normalizedValue
                                    * (PlotBottom - PlotTop)));
                    })
                .ToArray());

        MaximumValueText = Format(
            maximumValue,
            numberFormat,
            unit,
            culture);
        MinimumValueText = Format(
            minimumValue,
            numberFormat,
            unit,
            culture);
        _stations = Array.AsReadOnly(
            [
                CreateStation(
                    "Chamber",
                    profile.Chamber,
                    valueSelector,
                    numberFormat,
                    unit,
                    culture),
                CreateStation(
                    "Throat",
                    profile.Throat,
                    valueSelector,
                    numberFormat,
                    unit,
                    culture),
                CreateStation(
                    "Exit",
                    profile.Exit,
                    valueSelector,
                    numberFormat,
                    unit,
                    culture),
            ]);
        AccessibilitySummary =
            $"{Title} profile. "
            + string.Join(
                "; ",
                Stations.Select(
                    static station =>
                        $"{station.Name} {station.ValueText}"));
    }

    public string Title { get; }

    public string Unit { get; }

    public string StrokeColor { get; }

    public string MaximumValueText { get; }

    public string MinimumValueText { get; }

    public string AccessibilitySummary { get; }

    public IReadOnlyList<NozzleProfileChartPointViewModel>
        PlotPoints => _plotPoints;

    public IReadOnlyList<NozzleProfileStationValueViewModel>
        Stations => _stations;

    private static NozzleProfileStationValueViewModel CreateStation(
        string name,
        NozzleProfilePoint point,
        Func<NozzleProfilePoint, double> valueSelector,
        string numberFormat,
        string unit,
        CultureInfo culture)
    {
        return new NozzleProfileStationValueViewModel(
            name,
            point.NormalizedAxialPosition.ToString(
                "N2",
                culture),
            Format(
                valueSelector(point),
                numberFormat,
                unit,
                culture));
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
