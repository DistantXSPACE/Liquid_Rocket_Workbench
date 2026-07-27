using System.Globalization;
using LiquidRocketWorkbench.App.ViewModels;
using LiquidRocketWorkbench.Core.Calculations;
using LiquidRocketWorkbench.Core.Models;
using LiquidRocketWorkbench.Core.Validation;

namespace LiquidRocketWorkbench.App.Tests.ViewModels;

public sealed class NozzleStationDiagramViewModelTests
{
    [Fact]
    public void Create_WithNullInputs_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => NozzleStationDiagramViewModel.Create(
                null!,
                CalculationWorkflowState.Empty,
                detailedResult: null));
    }

    [Fact]
    public void Create_WithSuccessAndNoDetailedResult_Throws()
    {
        var inputs = CreateInputs();

        Assert.Throws<ArgumentException>(
            () => NozzleStationDiagramViewModel.Create(
                inputs,
                CalculationWorkflowState.Success,
                detailedResult: null));
    }

    [Fact]
    public void Create_EmptyState_ProjectsFourInputStations()
    {
        var diagram = NozzleStationDiagramViewModel.Create(
            CreateInputs(),
            CalculationWorkflowState.Empty,
            detailedResult: null);

        Assert.Equal(CalculationWorkflowState.Empty, diagram.WorkflowState);
        Assert.Equal("INPUT SCHEMATIC", diagram.StateLabel);
        Assert.False(diagram.HasSolvedExit);
        Assert.Contains(
            "not to scale",
            diagram.ScaleNotice,
            StringComparison.OrdinalIgnoreCase);
        Assert.Equal(
            ["01", "02", "03", "04"],
            diagram.Stations.Select(static station => station.StationNumber));
        Assert.Equal(
            ["Injector", "Chamber", "Throat", "Exit"],
            diagram.Stations.Select(static station => station.Name));
        Assert.Contains("O/F 3.5", diagram.Injector.ValueText);
        Assert.Contains("Pc 8 MPa", diagram.Chamber.ValueText);
        Assert.Contains("Tc 3500 K", diagram.Chamber.ValueText);
        Assert.Contains("dt 50 mm", diagram.Throat.ValueText);
        Assert.Contains("Mach 1", diagram.Throat.ValueText);
        Assert.Contains(
            "solution pending",
            diagram.Exit.ValueText,
            StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(CalculationWorkflowState.Loading, "SOLVING")]
    [InlineData(
        CalculationWorkflowState.Error,
        "SOLUTION UNAVAILABLE")]
    public void Create_IntermediateState_CommunicatesWorkflow(
        CalculationWorkflowState workflowState,
        string expectedLabel)
    {
        var diagram = NozzleStationDiagramViewModel.Create(
            CreateInputs(),
            workflowState,
            detailedResult: null);

        Assert.Equal(expectedLabel, diagram.StateLabel);
        Assert.False(diagram.HasSolvedExit);
        Assert.NotEmpty(diagram.StateSummary);
    }

    [Fact]
    public void Create_Success_AnnotatesExitWithSolvedValues()
    {
        var inputs = CreateInputs();
        var detailedResult = CalculateDetailedResult(inputs);

        var diagram = NozzleStationDiagramViewModel.Create(
            inputs,
            CalculationWorkflowState.Success,
            detailedResult);

        Assert.Equal(CalculationWorkflowState.Success, diagram.WorkflowState);
        Assert.Equal("SOLUTION ANNOTATED", diagram.StateLabel);
        Assert.True(diagram.HasSolvedExit);
        Assert.Contains(
            detailedResult.ExitMachNumber.ToString(
                "N3",
                CultureInfo.CurrentCulture),
            diagram.Exit.ValueText);
        Assert.Contains(
            detailedResult.ExitPressureKilopascals.ToString(
                "N3",
                CultureInfo.CurrentCulture),
            diagram.StateSummary);
        Assert.Contains(
            detailedResult.ExitTemperatureKelvin.ToString(
                "N1",
                CultureInfo.CurrentCulture),
            diagram.StateSummary);
        Assert.Contains(
            detailedResult.ExitVelocityMetersPerSecond.ToString(
                "N1",
                CultureInfo.CurrentCulture),
            diagram.StateSummary);
    }

    private static EngineInputViewModel CreateInputs()
    {
        return new EngineInputViewModel(new EngineInputsValidator());
    }

    private static DetailedPerformanceViewModel CalculateDetailedResult(
        EngineInputViewModel inputs)
    {
        Assert.True(inputs.TryCreateInputs(out var engineInputs));
        var outcome = new EnginePerformanceCalculator().Calculate(
            Assert.IsType<EngineInputs>(engineInputs));
        var performance = Assert.IsType<EnginePerformanceResult>(
            outcome.Performance);

        return new DetailedPerformanceViewModel(performance);
    }
}
