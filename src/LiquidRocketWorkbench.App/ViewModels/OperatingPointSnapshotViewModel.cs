using System.Globalization;
using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.App.ViewModels;

/// <summary>
/// Immutable display snapshot of one successful operating point and result.
/// </summary>
public sealed class OperatingPointSnapshotViewModel
{
    public OperatingPointSnapshotViewModel(
        int snapshotNumber,
        int sourceCalculationId,
        EngineInputs inputs,
        EnginePerformanceResult performance,
        Action<OperatingPointSnapshotViewModel> remove,
        CultureInfo? displayCulture = null)
    {
        if (snapshotNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(snapshotNumber),
                snapshotNumber,
                "Snapshot number must be positive.");
        }

        if (sourceCalculationId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sourceCalculationId),
                sourceCalculationId,
                "Source calculation ID must be positive.");
        }

        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(performance);
        ArgumentNullException.ThrowIfNull(remove);
        var culture = displayCulture ?? CultureInfo.CurrentCulture;

        SnapshotNumber = snapshotNumber;
        SourceCalculationId = sourceCalculationId;
        DisplayName = $"Point {snapshotNumber}";
        PropellantLabel = inputs.PropellantLabel;
        ChamberPressureMegapascals =
            inputs.ChamberPressurePascals / 1_000_000;
        ChamberTemperatureKelvin =
            inputs.Gas.ChamberTemperatureKelvin;
        MixtureRatio = inputs.MixtureRatio;
        SpecificHeatRatio = inputs.Gas.SpecificHeatRatio;
        SpecificGasConstantJoulesPerKilogramKelvin =
            inputs.Gas.SpecificGasConstantJoulesPerKilogramKelvin;
        ThroatDiameterMillimeters =
            inputs.Nozzle.ThroatDiameterMeters * 1000;
        ExitDiameterMillimeters =
            inputs.Nozzle.ExitDiameterMeters * 1000;
        ExpansionRatio = performance.Geometry.ExpansionRatio;
        AmbientPressureKilopascals =
            inputs.AmbientPressurePascals / 1000;

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
        ExitMachNumber = performance.NozzleExit.MachNumber;
        ExitPressureKilopascals =
            performance.NozzleExit.PressurePascals / 1000;
        NozzleExpansionStateLabel = ToStateLabel(
            performance.SelectedAmbientPerformance.NozzleExpansionState);
        WarningCount = performance.Diagnostics.Count(
            static issue =>
                issue.Severity
                == Core.Diagnostics.ValidationSeverity.Warning);

        AccessibilitySummary =
            $"{DisplayName}, {PropellantLabel}. "
            + $"Chamber {Format(
                ChamberPressureMegapascals,
                "N3",
                culture)} MPa, "
            + $"ambient {Format(
                AmbientPressureKilopascals,
                "N3",
                culture)} kPa, "
            + $"expansion ratio {Format(
                ExpansionRatio,
                "N2",
                culture)}. "
            + $"Selected thrust "
            + $"{Format(
                SelectedThrustKilonewtons,
                "N3",
                culture)} kN, "
            + $"specific impulse "
            + $"{Format(
                SelectedSpecificImpulseSeconds,
                "N1",
                culture)} s; "
            + $"vacuum thrust "
            + $"{Format(
                VacuumThrustKilonewtons,
                "N3",
                culture)} kN.";
        RemoveAccessibilityName = $"Remove {DisplayName}";
        RemoveCommand = new RelayCommand(() => remove(this));
    }

    public int SnapshotNumber { get; }

    public int SourceCalculationId { get; }

    public string DisplayName { get; }

    public string PropellantLabel { get; }

    public double ChamberPressureMegapascals { get; }

    public double ChamberTemperatureKelvin { get; }

    public double MixtureRatio { get; }

    public double SpecificHeatRatio { get; }

    public double SpecificGasConstantJoulesPerKilogramKelvin { get; }

    public double ThroatDiameterMillimeters { get; }

    public double ExitDiameterMillimeters { get; }

    public double ExpansionRatio { get; }

    public double AmbientPressureKilopascals { get; }

    public double SelectedThrustKilonewtons { get; }

    public double SelectedSpecificImpulseSeconds { get; }

    public double VacuumThrustKilonewtons { get; }

    public double VacuumSpecificImpulseSeconds { get; }

    public double SeaLevelThrustKilonewtons { get; }

    public double SeaLevelSpecificImpulseSeconds { get; }

    public double CalculatedMassFlowRateKilogramsPerSecond { get; }

    public double ExitMachNumber { get; }

    public double ExitPressureKilopascals { get; }

    public string NozzleExpansionStateLabel { get; }

    public int WarningCount { get; }

    public string WarningSummary =>
        WarningCount == 1
            ? "1 model warning"
            : $"{WarningCount} model warnings";

    public string AccessibilitySummary { get; }

    public string RemoveAccessibilityName { get; }

    public RelayCommand RemoveCommand { get; }

    private static string ToStateLabel(NozzleExpansionState state)
    {
        return state switch
        {
            NozzleExpansionState.Underexpanded => "Underexpanded",
            NozzleExpansionState.IdeallyExpanded => "Ideally expanded",
            NozzleExpansionState.Overexpanded => "Overexpanded",
            _ => "Unknown",
        };
    }

    private static string Format(
        double value,
        string numberFormat,
        CultureInfo culture)
    {
        return value.ToString(numberFormat, culture);
    }
}
