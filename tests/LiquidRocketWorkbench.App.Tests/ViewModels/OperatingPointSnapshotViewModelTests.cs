using LiquidRocketWorkbench.App.ViewModels;
using LiquidRocketWorkbench.Core;
using LiquidRocketWorkbench.Core.Calculations;
using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.App.Tests.ViewModels;

public sealed class OperatingPointSnapshotViewModelTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public void Constructor_WithInvalidIdentity_Throws(
        int snapshotNumber,
        int sourceCalculationId)
    {
        var (inputs, performance) = CreateOperatingPoint();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new OperatingPointSnapshotViewModel(
                snapshotNumber,
                sourceCalculationId,
                inputs,
                performance,
                static _ => { }));
    }

    [Fact]
    public void Constructor_WithNullRequiredValue_Throws()
    {
        var (inputs, performance) = CreateOperatingPoint();

        Assert.Throws<ArgumentNullException>(
            () => new OperatingPointSnapshotViewModel(
                1,
                1,
                null!,
                performance,
                static _ => { }));
        Assert.Throws<ArgumentNullException>(
            () => new OperatingPointSnapshotViewModel(
                1,
                1,
                inputs,
                null!,
                static _ => { }));
        Assert.Throws<ArgumentNullException>(
            () => new OperatingPointSnapshotViewModel(
                1,
                1,
                inputs,
                performance,
                null!));
    }

    [Fact]
    public void Constructor_CopiesInputsAndPerformanceIntoDisplaySnapshot()
    {
        var (inputs, performance) = CreateOperatingPoint();

        var snapshot = new OperatingPointSnapshotViewModel(
            snapshotNumber: 2,
            sourceCalculationId: 7,
            inputs,
            performance,
            static _ => { });

        Assert.Equal(2, snapshot.SnapshotNumber);
        Assert.Equal(7, snapshot.SourceCalculationId);
        Assert.Equal("Point 2", snapshot.DisplayName);
        Assert.Equal(inputs.PropellantLabel, snapshot.PropellantLabel);
        Assert.Equal(8, snapshot.ChamberPressureMegapascals);
        Assert.Equal(3500, snapshot.ChamberTemperatureKelvin);
        Assert.Equal(3.5, snapshot.MixtureRatio);
        Assert.Equal(1.22, snapshot.SpecificHeatRatio);
        Assert.Equal(
            355,
            snapshot.SpecificGasConstantJoulesPerKilogramKelvin);
        Assert.Equal(50, snapshot.ThroatDiameterMillimeters, 10);
        Assert.Equal(
            316.22776601683796,
            snapshot.ExitDiameterMillimeters,
            10);
        Assert.Equal(40, snapshot.ExpansionRatio, 10);
        Assert.Equal(101.325, snapshot.AmbientPressureKilopascals);
        Assert.Equal(
            performance.SelectedAmbientPerformance.TotalThrustNewtons / 1000,
            snapshot.SelectedThrustKilonewtons,
            10);
        Assert.Equal(
            performance.VacuumPerformance.TotalThrustNewtons / 1000,
            snapshot.VacuumThrustKilonewtons,
            10);
        Assert.Equal(
            performance.NozzleExit.MachNumber,
            snapshot.ExitMachNumber);
        Assert.Equal("Overexpanded", snapshot.NozzleExpansionStateLabel);
        Assert.Equal(2, snapshot.WarningCount);
    }

    [Fact]
    public void RemoveCommand_PassesThisSnapshotToOwner()
    {
        var (inputs, performance) = CreateOperatingPoint();
        OperatingPointSnapshotViewModel? removed = null;
        var snapshot = new OperatingPointSnapshotViewModel(
            1,
            1,
            inputs,
            performance,
            candidate => removed = candidate);

        snapshot.RemoveCommand.Execute(null);

        Assert.Same(snapshot, removed);
        Assert.Equal("Remove Point 1", snapshot.RemoveAccessibilityName);
    }

    [Fact]
    public void AccessibilitySummary_ContainsInputAndResultIdentity()
    {
        var (inputs, performance) = CreateOperatingPoint();
        var snapshot = new OperatingPointSnapshotViewModel(
            1,
            1,
            inputs,
            performance,
            static _ => { });

        Assert.Contains("Point 1", snapshot.AccessibilitySummary);
        Assert.Contains(inputs.PropellantLabel, snapshot.AccessibilitySummary);
        Assert.Contains("8.000 MPa", snapshot.AccessibilitySummary);
        Assert.Contains("101.325 kPa", snapshot.AccessibilitySummary);
        Assert.Contains("21.315 kN", snapshot.AccessibilitySummary);
    }

    private static (EngineInputs Inputs, EnginePerformanceResult Performance)
        CreateOperatingPoint()
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
                PhysicalConstants.StandardSeaLevelPressurePascals);
        var outcome = new EnginePerformanceCalculator().Calculate(inputs);

        Assert.True(outcome.IsSuccess);
        return (
            inputs,
            Assert.IsType<EnginePerformanceResult>(outcome.Performance));
    }
}
