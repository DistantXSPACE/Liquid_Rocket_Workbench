namespace LiquidRocketWorkbench.Core.Validation;

/// <summary>
/// Stable field identifiers shared by Core validation and UI input mapping.
/// </summary>
public static class EngineInputFields
{
    public const string PropellantLabel = "propellantLabel";
    public const string ChamberPressure = "chamberPressurePascals";
    public const string MixtureRatio = "mixtureRatio";
    public const string Nozzle = "nozzle";
    public const string ThroatDiameter = "nozzle.throatDiameterMeters";
    public const string ExitDiameter = "nozzle.exitDiameterMeters";
    public const string Gas = "gas";
    public const string ChamberTemperature = "gas.chamberTemperatureKelvin";
    public const string SpecificHeatRatio = "gas.specificHeatRatio";
    public const string SpecificGasConstant =
        "gas.specificGasConstantJoulesPerKilogramKelvin";
    public const string AmbientPressure = "ambientPressurePascals";
    public const string TargetMassFlowRate =
        "targetMassFlowRateKilogramsPerSecond";
    public const string BurnDuration = "burnDurationSeconds";
}
