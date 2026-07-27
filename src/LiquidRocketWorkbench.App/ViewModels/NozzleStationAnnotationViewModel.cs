namespace LiquidRocketWorkbench.App.ViewModels;

/// <summary>
/// One labeled location on the schematic engine/nozzle flow path.
/// </summary>
public sealed record NozzleStationAnnotationViewModel(
    string StationNumber,
    string Name,
    string Role,
    string ValueText)
{
    public string AccessibilityName => $"{Name} station";
}
