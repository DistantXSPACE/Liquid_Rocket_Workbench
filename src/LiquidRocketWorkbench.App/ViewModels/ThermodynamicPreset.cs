namespace LiquidRocketWorkbench.App.ViewModels;

/// <summary>
/// An editable starting point for the MVP constant-property gas model.
/// </summary>
public sealed record ThermodynamicPreset(
    string Id,
    string DisplayName,
    string PropellantLabel,
    double? MixtureRatio,
    double? ChamberTemperatureKelvin,
    double? SpecificHeatRatio,
    double? SpecificGasConstantJoulesPerKilogramKelvin,
    string SourceSummary)
{
    public bool IsCustom => MixtureRatio is null;

    public override string ToString()
    {
        return DisplayName;
    }
}
