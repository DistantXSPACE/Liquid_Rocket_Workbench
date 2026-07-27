using LiquidRocketWorkbench.App.ViewModels;
using LiquidRocketWorkbench.Core;
using LiquidRocketWorkbench.Core.Calculations;
using LiquidRocketWorkbench.Core.Diagnostics;
using LiquidRocketWorkbench.Core.Models;
using LiquidRocketWorkbench.Core.Validation;

namespace LiquidRocketWorkbench.App.Tests.ViewModels;

public sealed class DetailedPerformanceViewModelTests
{
    [Fact]
    public void Constructor_WithNullPerformance_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => new DetailedPerformanceViewModel(null!));
    }

    [Fact]
    public void Constructor_ProjectsGeometryExitFlowAndOptionalValues()
    {
        var performance = CreatePerformance(includeOptionalValues: true);

        var viewModel = new DetailedPerformanceViewModel(performance);

        Assert.Equal(
            performance.Geometry.ThroatAreaSquareMeters * 10_000,
            viewModel.ThroatAreaSquareCentimeters,
            10);
        Assert.Equal(
            performance.Geometry.ExitAreaSquareMeters * 10_000,
            viewModel.ExitAreaSquareCentimeters,
            10);
        Assert.Equal(40, viewModel.ExpansionRatio, 10);
        Assert.Equal(
            performance.NozzleExit.PressurePascals / 1000,
            viewModel.ExitPressureKilopascals,
            10);
        Assert.Equal(
            performance.NozzleExit.MachNumber,
            viewModel.ExitMachNumber);
        Assert.Equal(
            performance.NozzleExit.TemperatureKelvin,
            viewModel.ExitTemperatureKelvin);
        Assert.Equal(
            performance.NozzleExit.VelocityMetersPerSecond,
            viewModel.ExitVelocityMetersPerSecond);
        Assert.Equal(
            performance.MassFlow.CalculatedMassFlowRateKilogramsPerSecond,
            viewModel.CalculatedMassFlowRateKilogramsPerSecond);
        Assert.Equal(
            performance.MassFlow.OxidizerMassFlowRateKilogramsPerSecond,
            viewModel.OxidizerMassFlowRateKilogramsPerSecond);
        Assert.Equal(
            performance.MassFlow.FuelMassFlowRateKilogramsPerSecond,
            viewModel.FuelMassFlowRateKilogramsPerSecond);
        Assert.Equal(20, viewModel.TargetMassFlowRateKilogramsPerSecond);
        Assert.Equal(
            performance.MassFlow.AbsoluteTargetDifferenceKilogramsPerSecond,
            viewModel.AbsoluteTargetDifferenceKilogramsPerSecond);
        Assert.Equal(
            performance.MassFlow.RelativeTargetDifference * 100,
            viewModel.RelativeTargetDifferencePercent);
        Assert.Equal(
            performance.MassFlow.PropellantMassConsumedKilograms,
            viewModel.PropellantMassConsumedKilograms);
        Assert.Equal(
            performance.CharacteristicVelocityMetersPerSecond,
            viewModel.CharacteristicVelocityMetersPerSecond);
        Assert.True(viewModel.HasTargetComparison);
        Assert.True(viewModel.HasPropellantMassConsumed);
    }

    [Fact]
    public void Constructor_ProjectsAmbientCasesInDocumentedOrder()
    {
        var performance = CreatePerformance(includeOptionalValues: true);

        var viewModel = new DetailedPerformanceViewModel(performance);

        Assert.Equal(
            ["Selected ambient", "Vacuum", "Sea level"],
            viewModel.AmbientCases.Select(static item => item.Label));
        Assert.Equal(
            101.325,
            viewModel.AmbientCases[0].AmbientPressureKilopascals);
        Assert.Equal(
            performance.SelectedAmbientPerformance.TotalThrustNewtons / 1000,
            viewModel.AmbientCases[0].TotalThrustKilonewtons,
            10);
        Assert.Equal(
            performance.VacuumPerformance.PressureThrustNewtons / 1000,
            viewModel.AmbientCases[1].PressureThrustKilonewtons,
            10);
        Assert.Equal(
            NozzleExpansionState.Underexpanded,
            viewModel.AmbientCases[1].NozzleExpansionState);
        Assert.Equal(
            "Underexpanded",
            viewModel.AmbientCases[1].NozzleExpansionStateLabel);
        Assert.Equal(
            performance.SeaLevelPerformance.ThrustCoefficient,
            viewModel.AmbientCases[2].ThrustCoefficient);
    }

    [Fact]
    public void Constructor_PreservesOrderedCoreDiagnostics()
    {
        var performance = CreatePerformance(includeOptionalValues: true);

        var viewModel = new DetailedPerformanceViewModel(performance);

        Assert.Equal(
            [
                CalculationDiagnosticCodes.IdealFlowAssumptions,
                CalculationDiagnosticCodes.SevereOverexpansionLimit,
                CalculationDiagnosticCodes.TargetMassFlowMismatch,
            ],
            viewModel.Diagnostics.Select(static item => item.Code));
        Assert.All(
            viewModel.Diagnostics,
            static item =>
            {
                Assert.Equal(ValidationSeverity.Warning, item.Severity);
                Assert.True(item.IsWarning);
            });
        Assert.Equal(3, viewModel.WarningCount);
        Assert.True(viewModel.HasDiagnostics);
        Assert.Equal(
            "3 model warnings accompany this ideal result.",
            viewModel.DiagnosticSummary);
        Assert.Equal(
            "Target mass flow",
            viewModel.Diagnostics[2].FieldLabel);
    }

    [Fact]
    public void Constructor_WithoutOptionalInputs_LeavesOptionalValuesAbsent()
    {
        var performance = CreatePerformance(includeOptionalValues: false);

        var viewModel = new DetailedPerformanceViewModel(performance);

        Assert.False(viewModel.HasTargetComparison);
        Assert.False(viewModel.HasPropellantMassConsumed);
        Assert.Null(viewModel.TargetMassFlowRateKilogramsPerSecond);
        Assert.Null(viewModel.AbsoluteTargetDifferenceKilogramsPerSecond);
        Assert.Null(viewModel.RelativeTargetDifferencePercent);
        Assert.Null(viewModel.PropellantMassConsumedKilograms);
        Assert.DoesNotContain(
            viewModel.Diagnostics,
            static item =>
                item.Code
                == CalculationDiagnosticCodes.TargetMassFlowMismatch);
    }

    [Fact]
    public void AmbientDetail_RejectsInvalidRequiredValues()
    {
        var performance = CreatePerformance(includeOptionalValues: false);

        Assert.Throws<ArgumentException>(
            () => new AmbientPerformanceDetailViewModel(
                " ",
                performance.VacuumPerformance));
        Assert.Throws<ArgumentNullException>(
            () => new AmbientPerformanceDetailViewModel(
                "Vacuum",
                null!));
    }

    [Fact]
    public void DiagnosticPresentation_MapsSeverityAndTargetField()
    {
        var issue = new ValidationIssue(
            "TEST.WARNING",
            ValidationSeverity.Warning,
            "Review the target.",
            EngineInputFields.TargetMassFlowRate);

        var viewModel = new DiagnosticPresentationViewModel(issue);

        Assert.Equal("TEST.WARNING", viewModel.Code);
        Assert.Equal("Warning", viewModel.SeverityLabel);
        Assert.Equal("Review the target.", viewModel.Message);
        Assert.Equal("Target mass flow", viewModel.FieldLabel);
        Assert.True(viewModel.HasField);
        Assert.True(viewModel.IsWarning);
        Assert.False(viewModel.IsInformation);
        Assert.False(viewModel.IsError);
    }

    [Fact]
    public void DiagnosticPresentation_WithNullIssue_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => new DiagnosticPresentationViewModel(null!));
    }

    private static EnginePerformanceResult CreatePerformance(
        bool includeOptionalValues)
    {
        var inputs = new EngineInputs(
            PropellantLabel: "VC-04 synthetic constant-property gas",
            ChamberPressurePascals: 8_000_000,
            MixtureRatio: 3.5,
            Nozzle: new NozzleGeometry(
                ThroatDiameterMeters: 0.05,
                ExitDiameterMeters: 0.31622776601683794),
            Gas: new GasProperties(
                ChamberTemperatureKelvin: 3500,
                SpecificHeatRatio: 1.22,
                SpecificGasConstantJoulesPerKilogramKelvin: 355),
            AmbientPressurePascals:
                PhysicalConstants.StandardSeaLevelPressurePascals,
            TargetMassFlowRateKilogramsPerSecond:
                includeOptionalValues ? 20 : null,
            BurnDurationSeconds:
                includeOptionalValues ? 10 : null);
        var outcome = new EnginePerformanceCalculator().Calculate(inputs);

        Assert.True(outcome.IsSuccess);
        return Assert.IsType<EnginePerformanceResult>(outcome.Performance);
    }
}
