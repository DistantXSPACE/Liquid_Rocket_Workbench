using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.App.ViewModels;

/// <summary>
/// User-facing headline projection of one complete Core performance result.
/// </summary>
public sealed class HeadlinePerformanceViewModel
{
    public HeadlinePerformanceViewModel(
        string propellantLabel,
        EnginePerformanceResult performance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propellantLabel);
        ArgumentNullException.ThrowIfNull(performance);

        PropellantLabel = propellantLabel;
        SelectedAmbientPressureKilopascals =
            performance.SelectedAmbientPerformance.AmbientPressurePascals
            / 1000;
        SelectedThrustKilonewtons =
            performance.SelectedAmbientPerformance.TotalThrustNewtons
            / 1000;
        SelectedSpecificImpulseSeconds =
            performance.SelectedAmbientPerformance.SpecificImpulseSeconds;
        VacuumThrustKilonewtons =
            performance.VacuumPerformance.TotalThrustNewtons / 1000;
        VacuumSpecificImpulseSeconds =
            performance.VacuumPerformance.SpecificImpulseSeconds;
        SeaLevelThrustKilonewtons =
            performance.SeaLevelPerformance.TotalThrustNewtons / 1000;
        SeaLevelSpecificImpulseSeconds =
            performance.SeaLevelPerformance.SpecificImpulseSeconds;
        CalculatedMassFlowRateKilogramsPerSecond =
            performance.MassFlow.CalculatedMassFlowRateKilogramsPerSecond;
        NozzleExpansionState =
            performance.SelectedAmbientPerformance.NozzleExpansionState;
    }

    public string PropellantLabel { get; }

    public double SelectedAmbientPressureKilopascals { get; }

    public double SelectedThrustKilonewtons { get; }

    public double SelectedSpecificImpulseSeconds { get; }

    public double VacuumThrustKilonewtons { get; }

    public double VacuumSpecificImpulseSeconds { get; }

    public double SeaLevelThrustKilonewtons { get; }

    public double SeaLevelSpecificImpulseSeconds { get; }

    public double CalculatedMassFlowRateKilogramsPerSecond { get; }

    public NozzleExpansionState NozzleExpansionState { get; }

    public string NozzleExpansionStateLabel =>
        NozzleExpansionState switch
        {
            NozzleExpansionState.Underexpanded => "Underexpanded",
            NozzleExpansionState.IdeallyExpanded => "Ideally expanded",
            NozzleExpansionState.Overexpanded => "Overexpanded",
            _ => "Unknown",
        };
}
