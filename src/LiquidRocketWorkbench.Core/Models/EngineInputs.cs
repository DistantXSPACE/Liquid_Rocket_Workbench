namespace LiquidRocketWorkbench.Core.Models;

/// <summary>
/// Immutable operating-point inputs expressed in canonical SI units.
/// </summary>
public sealed record EngineInputs(
    string PropellantLabel,
    double ChamberPressurePascals,
    double MixtureRatio,
    NozzleGeometry Nozzle,
    GasProperties Gas,
    double AmbientPressurePascals,
    double? TargetMassFlowRateKilogramsPerSecond = null,
    double? BurnDurationSeconds = null);
