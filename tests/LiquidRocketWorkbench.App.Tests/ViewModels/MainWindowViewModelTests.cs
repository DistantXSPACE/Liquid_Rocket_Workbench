using LiquidRocketWorkbench.App.ViewModels;
using LiquidRocketWorkbench.Core.Calculations;
using LiquidRocketWorkbench.Core.Models;
using LiquidRocketWorkbench.Core.Validation;

namespace LiquidRocketWorkbench.App.Tests.ViewModels;

public sealed class MainWindowViewModelTests
{
    [Fact]
    public void Constructor_WithNullCalculator_Throws()
    {
        var inputs = CreateInputs();

        Assert.Throws<ArgumentNullException>(
            () => new MainWindowViewModel(null!, inputs));
    }

    [Fact]
    public void Constructor_WithNullInputs_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => new MainWindowViewModel(
                new EnginePerformanceCalculator(),
                null!));
    }

    [Fact]
    public void Constructor_WithNegativeLoadingDuration_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new MainWindowViewModel(
                new EnginePerformanceCalculator(),
                CreateInputs(),
                TimeSpan.FromMilliseconds(-1)));
    }

    [Fact]
    public void InitialState_IsEmptyAndCommandIsEnabled()
    {
        var viewModel = CreateViewModel();

        AssertState(viewModel, CalculationWorkflowState.Empty);
        Assert.Equal("Ready to calculate", viewModel.EmptyStateTitle);
        Assert.Contains(
            "valid",
            viewModel.EmptyStateMessage,
            StringComparison.OrdinalIgnoreCase);
        Assert.Equal(
            "Calculate performance",
            viewModel.CalculateButtonText);
        Assert.True(viewModel.CalculateCommand.CanExecute(null));
        Assert.True(viewModel.CanEditInputs);
    }

    [Fact]
    public void InitialComparisonState_IsEmptyAndCommandsAreDisabled()
    {
        var viewModel = CreateViewModel();

        Assert.Empty(viewModel.SavedOperatingPoints);
        Assert.True(viewModel.IsOperatingPointComparisonEmpty);
        Assert.False(viewModel.HasSavedOperatingPoints);
        Assert.Equal("0 of 4 points saved", viewModel.SavedOperatingPointCountText);
        Assert.Equal("Calculate to save", viewModel.SaveOperatingPointButtonText);
        Assert.False(
            viewModel.SaveCurrentOperatingPointCommand.CanExecute(null));
        Assert.False(
            viewModel.ClearOperatingPointComparisonsCommand.CanExecute(null));
    }

    [Fact]
    public async Task Calculate_WithDefaultInputs_ProjectsReferenceHeadlines()
    {
        var calculator = new CountingCalculator();
        var viewModel = CreateViewModel(calculator);

        await viewModel.CalculateCommand.ExecuteAsync();

        var result = Assert.IsType<HeadlinePerformanceViewModel>(
            viewModel.HeadlineResult);
        var detailedResult = Assert.IsType<DetailedPerformanceViewModel>(
            viewModel.DetailedResult);
        Assert.Equal(1, calculator.CallCount);
        Assert.NotNull(calculator.LastInputs);
        Assert.Equal(8_000_000, calculator.LastInputs.ChamberPressurePascals);
        Assert.Equal(21.314758169192588, result.SelectedThrustKilonewtons, 10);
        Assert.Equal(
            236.41943206867248,
            result.SelectedSpecificImpulseSeconds,
            10);
        Assert.Equal(29.27280505981723, result.VacuumThrustKilonewtons, 10);
        Assert.Equal(
            324.6886449456306,
            result.VacuumSpecificImpulseSeconds,
            10);
        Assert.Equal(21.314758169192588, result.SeaLevelThrustKilonewtons, 10);
        Assert.Equal(
            236.41943206867248,
            result.SeaLevelSpecificImpulseSeconds,
            10);
        Assert.Equal(
            9.193408634242926,
            result.CalculatedMassFlowRateKilogramsPerSecond,
            10);
        Assert.Equal(101.325, result.SelectedAmbientPressureKilopascals);
        Assert.Equal(
            NozzleExpansionState.Overexpanded,
            result.NozzleExpansionState);
        Assert.Equal(
            "Overexpanded",
            result.NozzleExpansionStateLabel);
        Assert.Equal(3, detailedResult.WarningCount);
        AssertState(viewModel, CalculationWorkflowState.Success);
        Assert.Equal(
            "Recalculate performance",
            viewModel.CalculateButtonText);
    }

    [Fact]
    public async Task InvalidInput_DisablesCommandAndDoesNotCallCalculator()
    {
        var calculator = new CountingCalculator();
        var viewModel = CreateViewModel(calculator);

        viewModel.Inputs.ChamberPressureMegapascals = "invalid";

        Assert.False(viewModel.CalculateCommand.CanExecute(null));
        Assert.Equal(
            "Complete the operating point",
            viewModel.EmptyStateTitle);

        await viewModel.CalculateCommand.ExecuteAsync();

        Assert.Equal(0, calculator.CallCount);
        AssertState(viewModel, CalculationWorkflowState.Empty);
    }

    [Fact]
    public void CorrectingInput_ReenablesCommandAndRaisesCanExecuteChanged()
    {
        var viewModel = CreateViewModel();
        var notifications = 0;
        viewModel.CalculateCommand.CanExecuteChanged += (_, _) =>
            notifications++;

        viewModel.Inputs.MixtureRatio = "invalid";
        Assert.False(viewModel.CalculateCommand.CanExecute(null));

        viewModel.Inputs.MixtureRatio = "3.5";

        Assert.True(viewModel.CalculateCommand.CanExecute(null));
        Assert.True(notifications > 0);
        Assert.Equal("Ready to calculate", viewModel.EmptyStateTitle);
    }

    [Fact]
    public async Task EditingInputAfterSuccess_ClearsStaleResult()
    {
        var viewModel = CreateViewModel();
        await viewModel.CalculateCommand.ExecuteAsync();
        AssertState(viewModel, CalculationWorkflowState.Success);

        viewModel.Inputs.AmbientPressureKilopascals = "90";

        AssertState(viewModel, CalculationWorkflowState.Empty);
        Assert.Contains(
            "Ready",
            viewModel.StatusText,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task SelectingPresetAfterSuccess_ClearsResultAndUsesPresetLabel()
    {
        var viewModel = CreateViewModel();
        await viewModel.CalculateCommand.ExecuteAsync();
        AssertState(viewModel, CalculationWorkflowState.Success);
        var preset = viewModel.Inputs.Presets.Single(
            static candidate =>
                candidate.Id
                    == ThermodynamicPresetCatalog.LoxHydrogenReferenceId);

        viewModel.Inputs.SelectedPreset = preset;

        AssertState(viewModel, CalculationWorkflowState.Empty);
        Assert.True(viewModel.CalculateCommand.CanExecute(null));

        await viewModel.CalculateCommand.ExecuteAsync();

        var result = Assert.IsType<HeadlinePerformanceViewModel>(
            viewModel.HeadlineResult);
        Assert.Equal(
            "LOX / Hydrogen (reference estimate)",
            result.PropellantLabel);
        AssertState(viewModel, CalculationWorkflowState.Success);
    }

    [Fact]
    public async Task CalculationFailure_ShowsStructuredCoreMessageWithoutResult()
    {
        var viewModel = CreateViewModel();
        viewModel.Inputs.ThroatDiameterMillimeters = "0.001";
        viewModel.Inputs.ExitDiameterMillimeters = "1000000000000000";
        Assert.True(viewModel.CalculateCommand.CanExecute(null));

        await viewModel.CalculateCommand.ExecuteAsync();

        AssertState(viewModel, CalculationWorkflowState.Error);
        Assert.Contains(
            "area-Mach root",
            viewModel.CalculationErrorMessage,
            StringComparison.OrdinalIgnoreCase);
        Assert.Equal(
            "Try calculation again",
            viewModel.CalculateButtonText);
    }

    [Fact]
    public async Task UnexpectedCalculatorException_ShowsErrorState()
    {
        var viewModel = CreateViewModel(new ThrowingCalculator());

        await viewModel.CalculateCommand.ExecuteAsync();

        AssertState(viewModel, CalculationWorkflowState.Error);
        Assert.Contains(
            "Injected calculator failure",
            viewModel.CalculationErrorMessage,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task ErrorState_CanRetryIntoSuccess()
    {
        var calculator = new FailOnceCalculator();
        var viewModel = CreateViewModel(calculator);

        await viewModel.CalculateCommand.ExecuteAsync();
        AssertState(viewModel, CalculationWorkflowState.Error);
        Assert.True(viewModel.CalculateCommand.CanExecute(null));

        await viewModel.CalculateCommand.ExecuteAsync();

        Assert.Equal(2, calculator.CallCount);
        AssertState(viewModel, CalculationWorkflowState.Success);
    }

    [Fact]
    public async Task RunningCalculation_ShowsLoadingAndRejectsReentry()
    {
        using var calculator = new DeferredCalculator();
        var viewModel = CreateViewModel(calculator);

        var firstExecution = viewModel.CalculateCommand.ExecuteAsync();
        await calculator.Entered.WaitAsync(TimeSpan.FromSeconds(5));

        AssertState(viewModel, CalculationWorkflowState.Loading);
        Assert.False(viewModel.CalculateCommand.CanExecute(null));
        Assert.False(viewModel.CanEditInputs);
        Assert.Equal("Calculating…", viewModel.CalculateButtonText);
        Assert.Contains(
            "solver running",
            viewModel.StatusText,
            StringComparison.Ordinal);

        await viewModel.CalculateCommand.ExecuteAsync();
        Assert.Equal(1, calculator.CallCount);

        calculator.Release();
        await firstExecution;

        AssertState(viewModel, CalculationWorkflowState.Success);
        Assert.True(viewModel.CalculateCommand.CanExecute(null));
    }

    [Fact]
    public async Task InputChangeDuringCalculation_DiscardsStaleResult()
    {
        using var calculator = new DeferredCalculator();
        var viewModel = CreateViewModel(calculator);
        var execution = viewModel.CalculateCommand.ExecuteAsync();
        await calculator.Entered.WaitAsync(TimeSpan.FromSeconds(5));
        AssertState(viewModel, CalculationWorkflowState.Loading);

        viewModel.Inputs.AmbientPressureKilopascals = "90";

        AssertState(viewModel, CalculationWorkflowState.Empty);
        calculator.Release();
        await execution;

        AssertState(viewModel, CalculationWorkflowState.Empty);
        Assert.Null(viewModel.HeadlineResult);
        Assert.Null(viewModel.DetailedResult);
    }

    [Fact]
    public async Task SuccessfulCalculation_RaisesWorkflowStateNotifications()
    {
        var viewModel = CreateViewModel();
        var changedProperties = new List<string?>();
        viewModel.PropertyChanged += (_, args) =>
            changedProperties.Add(args.PropertyName);

        await viewModel.CalculateCommand.ExecuteAsync();

        Assert.Contains(nameof(viewModel.HeadlineResult), changedProperties);
        Assert.Contains(nameof(viewModel.DetailedResult), changedProperties);
        Assert.Contains(nameof(viewModel.NozzleProfiles), changedProperties);
        Assert.Contains(nameof(viewModel.ThrustAltitudePlot), changedProperties);
        Assert.Contains(nameof(viewModel.WorkflowState), changedProperties);
        Assert.Contains(nameof(viewModel.IsLoadingState), changedProperties);
        Assert.Contains(nameof(viewModel.IsSuccessState), changedProperties);
        Assert.Contains(nameof(viewModel.HasResult), changedProperties);
        Assert.Contains(nameof(viewModel.IsAwaitingCalculation), changedProperties);
        Assert.Contains(nameof(viewModel.StatusText), changedProperties);
    }

    [Fact]
    public async Task NozzleDiagram_FollowsCalculationAndInputReset()
    {
        var viewModel = CreateViewModel();

        Assert.Equal("INPUT SCHEMATIC", viewModel.NozzleDiagram.StateLabel);
        Assert.Contains(
            "solution pending",
            viewModel.NozzleDiagram.Exit.ValueText,
            StringComparison.Ordinal);

        await viewModel.CalculateCommand.ExecuteAsync();

        Assert.Equal(
            "SOLUTION ANNOTATED",
            viewModel.NozzleDiagram.StateLabel);
        Assert.True(viewModel.NozzleDiagram.HasSolvedExit);
        Assert.Contains("Mach", viewModel.NozzleDiagram.Exit.ValueText);

        viewModel.Inputs.ExitDiameterMillimeters = "320";

        Assert.Equal("INPUT SCHEMATIC", viewModel.NozzleDiagram.StateLabel);
        Assert.False(viewModel.NozzleDiagram.HasSolvedExit);
        Assert.Contains("de 320 mm", viewModel.NozzleDiagram.Exit.ValueText);
        Assert.Contains(
            "solution pending",
            viewModel.NozzleDiagram.Exit.ValueText,
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task NozzleProfiles_AppearOnlyForCurrentSuccessfulResult()
    {
        var viewModel = CreateViewModel();

        Assert.Null(viewModel.NozzleProfiles);

        await viewModel.CalculateCommand.ExecuteAsync();

        var profiles = Assert.IsType<NozzleFlowProfileViewModel>(
            viewModel.NozzleProfiles);
        Assert.Equal(3, profiles.Series.Count);
        Assert.Contains(
            "4.356",
            profiles.Mach.Stations[2].ValueText.Replace(',', '.'));

        viewModel.Inputs.ChamberPressureMegapascals = "7.5";

        Assert.Null(viewModel.NozzleProfiles);
        AssertState(viewModel, CalculationWorkflowState.Empty);
    }

    [Fact]
    public async Task ThrustAltitudePlot_AppearsOnlyForCurrentSuccessfulResult()
    {
        var viewModel = CreateViewModel();

        Assert.Null(viewModel.ThrustAltitudePlot);

        await viewModel.CalculateCommand.ExecuteAsync();

        var plot = Assert.IsType<ThrustAltitudeViewModel>(
            viewModel.ThrustAltitudePlot);
        Assert.Equal(51, plot.CurvePoints.Count);
        Assert.Equal("21.315 kN", plot.Stations[0].ThrustText);

        viewModel.Inputs.AmbientPressureKilopascals = "90";

        Assert.Null(viewModel.ThrustAltitudePlot);
        AssertState(viewModel, CalculationWorkflowState.Empty);
    }

    [Fact]
    public async Task SaveComparison_CapturesResultWithoutChangingActiveInputs()
    {
        var viewModel = CreateViewModel();
        await viewModel.CalculateCommand.ExecuteAsync();
        var activeInputs = CaptureInputText(viewModel.Inputs);

        viewModel.SaveCurrentOperatingPointCommand.Execute(null);

        var snapshot = Assert.Single(viewModel.SavedOperatingPoints);
        Assert.Equal("Point 1", snapshot.DisplayName);
        Assert.Equal(activeInputs, CaptureInputText(viewModel.Inputs));
        Assert.True(viewModel.HasSavedOperatingPoints);
        Assert.False(viewModel.IsOperatingPointComparisonEmpty);
        Assert.Equal("1 of 4 points saved", viewModel.SavedOperatingPointCountText);
        Assert.Equal(
            "Current result saved",
            viewModel.SaveOperatingPointButtonText);
        Assert.False(
            viewModel.SaveCurrentOperatingPointCommand.CanExecute(null));
        Assert.True(
            viewModel.ClearOperatingPointComparisonsCommand.CanExecute(null));
    }

    [Fact]
    public async Task SavedComparison_SurvivesEditsAndLaterCalculation()
    {
        var viewModel = CreateViewModel();
        await viewModel.CalculateCommand.ExecuteAsync();
        viewModel.SaveCurrentOperatingPointCommand.Execute(null);
        var first = Assert.Single(viewModel.SavedOperatingPoints);

        viewModel.Inputs.ChamberPressureMegapascals = "7.5";

        Assert.Single(viewModel.SavedOperatingPoints);
        Assert.Same(first, viewModel.SavedOperatingPoints[0]);
        Assert.Equal(8, first.ChamberPressureMegapascals);
        Assert.Equal("Calculate to save", viewModel.SaveOperatingPointButtonText);

        await viewModel.CalculateCommand.ExecuteAsync();
        viewModel.SaveCurrentOperatingPointCommand.Execute(null);

        Assert.Equal(2, viewModel.SavedOperatingPoints.Count);
        Assert.Same(first, viewModel.SavedOperatingPoints[0]);
        Assert.Equal(8, viewModel.SavedOperatingPoints[0].ChamberPressureMegapascals);
        Assert.Equal(7.5, viewModel.SavedOperatingPoints[1].ChamberPressureMegapascals);
        Assert.NotEqual(
            viewModel.SavedOperatingPoints[0].SelectedThrustKilonewtons,
            viewModel.SavedOperatingPoints[1].SelectedThrustKilonewtons);
        Assert.Equal("7.5", viewModel.Inputs.ChamberPressureMegapascals);
    }

    [Fact]
    public async Task RemoveComparison_DoesNotChangeActiveInputsAndAllowsResave()
    {
        var viewModel = CreateViewModel();
        await viewModel.CalculateCommand.ExecuteAsync();
        viewModel.SaveCurrentOperatingPointCommand.Execute(null);
        var activeInputs = CaptureInputText(viewModel.Inputs);
        var snapshot = Assert.Single(viewModel.SavedOperatingPoints);

        snapshot.RemoveCommand.Execute(null);

        Assert.Empty(viewModel.SavedOperatingPoints);
        Assert.Equal(activeInputs, CaptureInputText(viewModel.Inputs));
        AssertState(viewModel, CalculationWorkflowState.Success);
        Assert.True(
            viewModel.SaveCurrentOperatingPointCommand.CanExecute(null));
        Assert.Equal(
            "Save current result",
            viewModel.SaveOperatingPointButtonText);
    }

    [Fact]
    public async Task ClearComparisons_DoesNotChangeActiveInputs()
    {
        var viewModel = CreateViewModel();
        await viewModel.CalculateCommand.ExecuteAsync();
        viewModel.SaveCurrentOperatingPointCommand.Execute(null);
        var activeInputs = CaptureInputText(viewModel.Inputs);

        viewModel.ClearOperatingPointComparisonsCommand.Execute(null);

        Assert.Empty(viewModel.SavedOperatingPoints);
        Assert.Equal(activeInputs, CaptureInputText(viewModel.Inputs));
        AssertState(viewModel, CalculationWorkflowState.Success);
        Assert.True(
            viewModel.SaveCurrentOperatingPointCommand.CanExecute(null));
    }

    [Fact]
    public async Task ComparisonSet_StopsAtFourWithoutChangingActiveInputs()
    {
        var viewModel = CreateViewModel();
        var ambientPressures = new[] { "101.325", "90", "80", "70" };

        foreach (var pressure in ambientPressures)
        {
            viewModel.Inputs.AmbientPressureKilopascals = pressure;
            await viewModel.CalculateCommand.ExecuteAsync();
            viewModel.SaveCurrentOperatingPointCommand.Execute(null);
        }

        viewModel.Inputs.AmbientPressureKilopascals = "60";
        await viewModel.CalculateCommand.ExecuteAsync();
        var activeInputs = CaptureInputText(viewModel.Inputs);

        Assert.Equal(
            MainWindowViewModel.MaximumSavedOperatingPointCount,
            viewModel.SavedOperatingPoints.Count);
        Assert.False(
            viewModel.SaveCurrentOperatingPointCommand.CanExecute(null));
        Assert.Equal("Comparison set full", viewModel.SaveOperatingPointButtonText);

        viewModel.SaveCurrentOperatingPointCommand.Execute(null);

        Assert.Equal(4, viewModel.SavedOperatingPoints.Count);
        Assert.Equal(activeInputs, CaptureInputText(viewModel.Inputs));
    }

    [Fact]
    public async Task SavedComparison_SurvivesLaterCalculationFailure()
    {
        var viewModel = CreateViewModel();
        await viewModel.CalculateCommand.ExecuteAsync();
        viewModel.SaveCurrentOperatingPointCommand.Execute(null);
        var snapshot = Assert.Single(viewModel.SavedOperatingPoints);

        viewModel.Inputs.ThroatDiameterMillimeters = "0.001";
        viewModel.Inputs.ExitDiameterMillimeters = "1000000000000000";
        await viewModel.CalculateCommand.ExecuteAsync();

        AssertState(viewModel, CalculationWorkflowState.Error);
        Assert.Same(snapshot, Assert.Single(viewModel.SavedOperatingPoints));
    }

    private static MainWindowViewModel CreateViewModel(
        IEnginePerformanceCalculator? calculator = null)
    {
        return new MainWindowViewModel(
            calculator ?? new EnginePerformanceCalculator(),
            CreateInputs(),
            minimumLoadingDuration: TimeSpan.Zero);
    }

    private static EngineInputViewModel CreateInputs()
    {
        return new EngineInputViewModel(new EngineInputsValidator());
    }

    private static string[] CaptureInputText(EngineInputViewModel inputs)
    {
        return
        [
            inputs.PropellantLabel,
            inputs.ChamberPressureMegapascals,
            inputs.ChamberTemperatureKelvin,
            inputs.SpecificHeatRatio,
            inputs.SpecificGasConstantJoulesPerKilogramKelvin,
            inputs.MixtureRatio,
            inputs.ThroatDiameterMillimeters,
            inputs.ExitDiameterMillimeters,
            inputs.AmbientPressureKilopascals,
            inputs.TargetMassFlowRateKilogramsPerSecond,
            inputs.BurnDurationSeconds,
        ];
    }

    private static void AssertState(
        MainWindowViewModel viewModel,
        CalculationWorkflowState expected)
    {
        Assert.Equal(expected, viewModel.WorkflowState);
        Assert.Equal(
            expected == CalculationWorkflowState.Empty,
            viewModel.IsEmptyState);
        Assert.Equal(
            expected == CalculationWorkflowState.Loading,
            viewModel.IsLoadingState);
        Assert.Equal(
            expected == CalculationWorkflowState.Error,
            viewModel.IsErrorState);
        Assert.Equal(
            expected == CalculationWorkflowState.Success,
            viewModel.IsSuccessState);
        Assert.Equal(viewModel.IsEmptyState, viewModel.IsAwaitingCalculation);
        Assert.Equal(viewModel.IsErrorState, viewModel.HasCalculationError);
        Assert.Equal(viewModel.IsSuccessState, viewModel.HasResult);

        if (expected == CalculationWorkflowState.Success)
        {
            Assert.NotNull(viewModel.HeadlineResult);
            Assert.NotNull(viewModel.DetailedResult);
            Assert.NotNull(viewModel.NozzleProfiles);
            Assert.NotNull(viewModel.ThrustAltitudePlot);
            Assert.Null(viewModel.CalculationErrorMessage);
        }
        else
        {
            Assert.Null(viewModel.HeadlineResult);
            Assert.Null(viewModel.DetailedResult);
            Assert.Null(viewModel.NozzleProfiles);
            Assert.Null(viewModel.ThrustAltitudePlot);
        }

        if (expected == CalculationWorkflowState.Error)
        {
            Assert.False(
                string.IsNullOrWhiteSpace(
                    viewModel.CalculationErrorMessage));
        }
        else
        {
            Assert.Null(viewModel.CalculationErrorMessage);
        }
    }

    private sealed class CountingCalculator : IEnginePerformanceCalculator
    {
        private readonly EnginePerformanceCalculator _inner = new();

        public int CallCount { get; private set; }

        public EngineInputs? LastInputs { get; private set; }

        public EngineCalculationResult Calculate(EngineInputs inputs)
        {
            CallCount++;
            LastInputs = inputs;
            return _inner.Calculate(inputs);
        }
    }

    private sealed class ThrowingCalculator : IEnginePerformanceCalculator
    {
        public EngineCalculationResult Calculate(EngineInputs inputs)
        {
            throw new InvalidOperationException(
                "Injected calculator failure.");
        }
    }

    private sealed class FailOnceCalculator : IEnginePerformanceCalculator
    {
        private readonly EnginePerformanceCalculator _inner = new();

        public int CallCount { get; private set; }

        public EngineCalculationResult Calculate(EngineInputs inputs)
        {
            CallCount++;
            if (CallCount == 1)
            {
                throw new InvalidOperationException(
                    "Injected first-attempt failure.");
            }

            return _inner.Calculate(inputs);
        }
    }

    private sealed class DeferredCalculator
        : IEnginePerformanceCalculator,
          IDisposable
    {
        private readonly EnginePerformanceCalculator _inner = new();
        private readonly TaskCompletionSource _entered =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly ManualResetEventSlim _release = new();
        private int _callCount;

        public Task Entered => _entered.Task;

        public int CallCount => Volatile.Read(ref _callCount);

        public EngineCalculationResult Calculate(EngineInputs inputs)
        {
            Interlocked.Increment(ref _callCount);
            _entered.TrySetResult();

            if (!_release.Wait(TimeSpan.FromSeconds(5)))
            {
                throw new TimeoutException(
                    "Deferred test calculator was not released.");
            }

            return _inner.Calculate(inputs);
        }

        public void Release()
        {
            _release.Set();
        }

        public void Dispose()
        {
            _release.Set();
            _release.Dispose();
        }
    }
}
