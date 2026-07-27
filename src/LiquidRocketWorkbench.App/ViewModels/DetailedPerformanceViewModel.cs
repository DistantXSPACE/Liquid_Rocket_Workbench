using LiquidRocketWorkbench.Core.Diagnostics;
using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.App.ViewModels;

/// <summary>
/// Complete metric display projection of a successful Core calculation.
/// </summary>
public sealed class DetailedPerformanceViewModel
{
    private readonly IReadOnlyList<AmbientPerformanceDetailViewModel>
        _ambientCases;
    private readonly IReadOnlyList<DiagnosticPresentationViewModel>
        _diagnostics;

    public DetailedPerformanceViewModel(
        EnginePerformanceResult performance)
    {
        ArgumentNullException.ThrowIfNull(performance);

        ThroatAreaSquareCentimeters =
            performance.Geometry.ThroatAreaSquareMeters * 10_000;
        ExitAreaSquareCentimeters =
            performance.Geometry.ExitAreaSquareMeters * 10_000;
        ExpansionRatio = performance.Geometry.ExpansionRatio;

        ExitMachNumber = performance.NozzleExit.MachNumber;
        ExitPressureKilopascals =
            performance.NozzleExit.PressurePascals / 1000;
        ExitTemperatureKelvin =
            performance.NozzleExit.TemperatureKelvin;
        ExitVelocityMetersPerSecond =
            performance.NozzleExit.VelocityMetersPerSecond;

        CalculatedMassFlowRateKilogramsPerSecond =
            performance.MassFlow.CalculatedMassFlowRateKilogramsPerSecond;
        OxidizerMassFlowRateKilogramsPerSecond =
            performance.MassFlow.OxidizerMassFlowRateKilogramsPerSecond;
        FuelMassFlowRateKilogramsPerSecond =
            performance.MassFlow.FuelMassFlowRateKilogramsPerSecond;
        TargetMassFlowRateKilogramsPerSecond =
            performance.MassFlow.TargetMassFlowRateKilogramsPerSecond;
        AbsoluteTargetDifferenceKilogramsPerSecond =
            performance.MassFlow.AbsoluteTargetDifferenceKilogramsPerSecond;
        RelativeTargetDifferencePercent =
            performance.MassFlow.RelativeTargetDifference * 100;
        PropellantMassConsumedKilograms =
            performance.MassFlow.PropellantMassConsumedKilograms;

        CharacteristicVelocityMetersPerSecond =
            performance.CharacteristicVelocityMetersPerSecond;

        _ambientCases = Array.AsReadOnly(
            [
                new AmbientPerformanceDetailViewModel(
                    "Selected ambient",
                    performance.SelectedAmbientPerformance),
                new AmbientPerformanceDetailViewModel(
                    "Vacuum",
                    performance.VacuumPerformance),
                new AmbientPerformanceDetailViewModel(
                    "Sea level",
                    performance.SeaLevelPerformance),
            ]);
        _diagnostics = Array.AsReadOnly(
            performance.Diagnostics
                .Select(
                    static issue =>
                        new DiagnosticPresentationViewModel(issue))
                .ToArray());
    }

    public double ThroatAreaSquareCentimeters { get; }

    public double ExitAreaSquareCentimeters { get; }

    public double ExpansionRatio { get; }

    public double ExitMachNumber { get; }

    public double ExitPressureKilopascals { get; }

    public double ExitTemperatureKelvin { get; }

    public double ExitVelocityMetersPerSecond { get; }

    public double CalculatedMassFlowRateKilogramsPerSecond { get; }

    public double OxidizerMassFlowRateKilogramsPerSecond { get; }

    public double FuelMassFlowRateKilogramsPerSecond { get; }

    public double? TargetMassFlowRateKilogramsPerSecond { get; }

    public double? AbsoluteTargetDifferenceKilogramsPerSecond { get; }

    public double? RelativeTargetDifferencePercent { get; }

    public double? PropellantMassConsumedKilograms { get; }

    public double CharacteristicVelocityMetersPerSecond { get; }

    public bool HasTargetComparison =>
        TargetMassFlowRateKilogramsPerSecond.HasValue;

    public bool HasPropellantMassConsumed =>
        PropellantMassConsumedKilograms.HasValue;

    public IReadOnlyList<AmbientPerformanceDetailViewModel>
        AmbientCases => _ambientCases;

    public IReadOnlyList<DiagnosticPresentationViewModel>
        Diagnostics => _diagnostics;

    public bool HasDiagnostics => Diagnostics.Count > 0;

    public int WarningCount =>
        Diagnostics.Count(
            static diagnostic =>
                diagnostic.Severity == ValidationSeverity.Warning);

    public string DiagnosticSummary =>
        WarningCount == 1
            ? "1 model warning accompanies this ideal result."
            : $"{WarningCount} model warnings accompany this ideal result.";
}
