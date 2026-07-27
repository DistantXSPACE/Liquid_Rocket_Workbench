using System.Globalization;
using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.App.ViewModels;

/// <summary>
/// Accessible small-multiple presentation of the normalized Core flow profile.
/// </summary>
public sealed class NozzleFlowProfileViewModel
{
    private readonly IReadOnlyList<NozzleProfileSeriesViewModel> _series;

    public NozzleFlowProfileViewModel(
        NozzleFlowProfileResult profile,
        CultureInfo? displayCulture = null)
    {
        ArgumentNullException.ThrowIfNull(profile);
        var culture = displayCulture ?? CultureInfo.CurrentCulture;

        Pressure = new NozzleProfileSeriesViewModel(
            "Static pressure",
            "kPa",
            "#B93B16",
            profile,
            static point => point.PressurePascals / 1000,
            "N1",
            culture);
        Temperature = new NozzleProfileSeriesViewModel(
            "Static temperature",
            "K",
            "#197A55",
            profile,
            static point => point.TemperatureKelvin,
            "N1",
            culture);
        Mach = new NozzleProfileSeriesViewModel(
            "Mach number",
            "Mach",
            "#2F6FA3",
            profile,
            static point => point.MachNumber,
            "N3",
            culture);
        _series = Array.AsReadOnly(
            [Pressure, Temperature, Mach]);
    }

    public NozzleProfileSeriesViewModel Pressure { get; }

    public NozzleProfileSeriesViewModel Temperature { get; }

    public NozzleProfileSeriesViewModel Mach { get; }

    public IReadOnlyList<NozzleProfileSeriesViewModel> Series => _series;

    public string AxisCaption =>
        "Normalized axial position: chamber 0.00 · throat 0.35 · exit 1.00";

    public string ModelNotice =>
        "Smooth station-to-station interpolation supports visualization only. "
        + "It is not a solved hardware contour, CFD result, or prediction of "
        + "boundary layers, shocks, or separation.";
}
