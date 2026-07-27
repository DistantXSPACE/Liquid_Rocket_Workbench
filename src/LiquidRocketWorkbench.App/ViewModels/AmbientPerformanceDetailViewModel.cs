using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.App.ViewModels;

/// <summary>
/// Display-unit projection of performance at one ambient condition.
/// </summary>
public sealed class AmbientPerformanceDetailViewModel
{
    public AmbientPerformanceDetailViewModel(
        string label,
        AmbientPerformanceResult performance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentNullException.ThrowIfNull(performance);

        Label = label;
        AmbientPressureKilopascals =
            performance.AmbientPressurePascals / 1000;
        MomentumThrustKilonewtons =
            performance.MomentumThrustNewtons / 1000;
        PressureThrustKilonewtons =
            performance.PressureThrustNewtons / 1000;
        TotalThrustKilonewtons =
            performance.TotalThrustNewtons / 1000;
        SpecificImpulseSeconds = performance.SpecificImpulseSeconds;
        ThrustCoefficient = performance.ThrustCoefficient;
        NozzleExpansionState = performance.NozzleExpansionState;
    }

    public string Label { get; }

    public double AmbientPressureKilopascals { get; }

    public double MomentumThrustKilonewtons { get; }

    public double PressureThrustKilonewtons { get; }

    public double TotalThrustKilonewtons { get; }

    public double SpecificImpulseSeconds { get; }

    public double ThrustCoefficient { get; }

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
