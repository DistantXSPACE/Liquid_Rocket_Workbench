namespace LiquidRocketWorkbench.App.ViewModels;

/// <summary>
/// Display value at one named nozzle profile station.
/// </summary>
public sealed record NozzleProfileStationValueViewModel(
    string Name,
    string PositionText,
    string ValueText);
