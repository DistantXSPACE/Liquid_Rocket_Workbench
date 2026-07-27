namespace LiquidRocketWorkbench.App.ViewModels;

/// <summary>
/// Display values at one standard-atmosphere altitude checkpoint.
/// </summary>
public sealed record ThrustAltitudeStationViewModel(
    string AltitudeText,
    string AmbientPressureText,
    string ThrustText);
