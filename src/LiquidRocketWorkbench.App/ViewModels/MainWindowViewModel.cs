using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using LiquidRocketWorkbench.Core.Calculations;
using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.App.ViewModels;

/// <summary>
/// Shell state and composed application services for the main window.
/// Owns the validated calculation workflow and headline result state.
/// </summary>
public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    public const int MaximumSavedOperatingPointCount = 4;

    private static readonly TimeSpan DefaultMinimumLoadingDuration =
        TimeSpan.FromMilliseconds(200);

    private readonly IEnginePerformanceCalculator _performanceCalculator;
    private readonly AsyncRelayCommand _calculateCommand;
    private readonly RelayCommand _saveCurrentOperatingPointCommand;
    private readonly RelayCommand _clearOperatingPointComparisonsCommand;
    private readonly ObservableCollection<OperatingPointSnapshotViewModel>
        _savedOperatingPoints = [];
    private readonly CultureInfo _displayCulture;
    private readonly TimeSpan _minimumLoadingDuration;
    private HeadlinePerformanceViewModel? _headlineResult;
    private DetailedPerformanceViewModel? _detailedResult;
    private NozzleFlowProfileViewModel? _nozzleProfiles;
    private ThrustAltitudeViewModel? _thrustAltitudePlot;
    private string? _calculationErrorMessage;
    private CalculationWorkflowState _workflowState =
        CalculationWorkflowState.Empty;
    private int _inputRevision;
    private int _successfulCalculationId;
    private int _nextSnapshotNumber = 1;
    private EngineInputs? _currentSuccessfulInputs;
    private EnginePerformanceResult? _currentSuccessfulPerformance;

    public MainWindowViewModel(
        IEnginePerformanceCalculator performanceCalculator,
        EngineInputViewModel inputs,
        TimeSpan? minimumLoadingDuration = null,
        CultureInfo? displayCulture = null)
    {
        ArgumentNullException.ThrowIfNull(performanceCalculator);
        ArgumentNullException.ThrowIfNull(inputs);

        var resolvedMinimumLoadingDuration =
            minimumLoadingDuration ?? DefaultMinimumLoadingDuration;
        if (resolvedMinimumLoadingDuration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minimumLoadingDuration),
                "Minimum loading duration cannot be negative.");
        }

        _performanceCalculator = performanceCalculator;
        _displayCulture = displayCulture ?? CultureInfo.CurrentCulture;
        _minimumLoadingDuration = resolvedMinimumLoadingDuration;
        Inputs = inputs;
        _calculateCommand = new AsyncRelayCommand(
            CalculateAsync,
            () => Inputs.IsInputValid);
        _saveCurrentOperatingPointCommand = new RelayCommand(
            SaveCurrentOperatingPoint,
            CanSaveCurrentOperatingPoint);
        SavedOperatingPoints =
            new ReadOnlyObservableCollection<OperatingPointSnapshotViewModel>(
                _savedOperatingPoints);
        _clearOperatingPointComparisonsCommand = new RelayCommand(
            ClearOperatingPointComparisons,
            () => SavedOperatingPoints.Count > 0);
        NozzleDiagram = NozzleStationDiagramViewModel.Create(
            Inputs,
            WorkflowState,
            detailedResult: null,
            _displayCulture);
        Inputs.PropertyChanged += HandleInputPropertyChanged;
        WorkflowSteps = Array.AsReadOnly(
            [
                new WorkflowStepViewModel(
                    StepNumber: "1",
                    Title: "Define inputs",
                    Description: "Operating conditions and nozzle geometry",
                    IsCurrent: true),
                new WorkflowStepViewModel(
                    StepNumber: "2",
                    Title: "Calculate",
                    Description: "Run the deterministic ideal-flow model",
                    IsCurrent: false),
                new WorkflowStepViewModel(
                    StepNumber: "3",
                    Title: "Review",
                    Description: "Performance values and model diagnostics",
                    IsCurrent: false),
                new WorkflowStepViewModel(
                    StepNumber: "4",
                    Title: "Explore",
                    Description: "Profiles and operating-point comparisons",
                    IsCurrent: false),
            ]);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string ApplicationTitle => "Liquid Rocket Workbench";

    public string ApplicationSubtitle =>
        "Idealized liquid-engine performance and nozzle analysis";

    public string ModelBadgeText => "1D · steady · isentropic";

    public string WorkspaceIntroduction =>
        "Build a complete operating point, calculate a deterministic ideal "
        + "solution, and keep every assumption and warning close to the "
        + "result.";

    public string CalculationServiceStatus =>
        _performanceCalculator is not null
            ? "Calculation core connected"
            : "Calculation core unavailable";

    public string CompactModelSummary =>
        "Constant-property ideal gas with choked throat flow.";

    public string FullModelSummary =>
        "The MVP uses one-dimensional, steady, adiabatic, isentropic ideal-gas "
        + "flow with constant properties. It does not predict combustion "
        + "chemistry, losses, shocks, separated flow, erosion, heat transfer, "
        + "or transient behavior.";

    public string StatusText =>
        WorkflowState switch
        {
            CalculationWorkflowState.Loading =>
                "Calculating · ideal-flow solver running",
            CalculationWorkflowState.Error =>
                "Calculation needs attention",
            CalculationWorkflowState.Success =>
                "Result current · ideal performance calculated",
            _ when Inputs.IsInputValid =>
                "Ready · valid inputs awaiting calculation",
            _ => "Input needs attention · resolve highlighted fields",
        };

    public string VersionLabel => ".NET 10 · WPF · SI calculation core";

    public EngineInputViewModel Inputs { get; }

    public AsyncRelayCommand CalculateCommand => _calculateCommand;

    public RelayCommand SaveCurrentOperatingPointCommand =>
        _saveCurrentOperatingPointCommand;

    public RelayCommand ClearOperatingPointComparisonsCommand =>
        _clearOperatingPointComparisonsCommand;

    public ReadOnlyObservableCollection<OperatingPointSnapshotViewModel>
    SavedOperatingPoints
    { get; }

    public CalculationWorkflowState WorkflowState => _workflowState;

    public NozzleStationDiagramViewModel NozzleDiagram { get; private set; }

    public HeadlinePerformanceViewModel? HeadlineResult
    {
        get => _headlineResult;
        private set
        {
            if (ReferenceEquals(_headlineResult, value))
            {
                return;
            }

            _headlineResult = value;
            OnPropertyChanged();
        }
    }

    public string? CalculationErrorMessage
    {
        get => _calculationErrorMessage;
        private set
        {
            if (_calculationErrorMessage == value)
            {
                return;
            }

            _calculationErrorMessage = value;
            OnPropertyChanged();
        }
    }

    public DetailedPerformanceViewModel? DetailedResult
    {
        get => _detailedResult;
        private set
        {
            if (ReferenceEquals(_detailedResult, value))
            {
                return;
            }

            _detailedResult = value;
            OnPropertyChanged();
        }
    }

    public NozzleFlowProfileViewModel? NozzleProfiles
    {
        get => _nozzleProfiles;
        private set
        {
            if (ReferenceEquals(_nozzleProfiles, value))
            {
                return;
            }

            _nozzleProfiles = value;
            OnPropertyChanged();
        }
    }

    public ThrustAltitudeViewModel? ThrustAltitudePlot
    {
        get => _thrustAltitudePlot;
        private set
        {
            if (ReferenceEquals(_thrustAltitudePlot, value))
            {
                return;
            }

            _thrustAltitudePlot = value;
            OnPropertyChanged();
        }
    }

    public bool IsEmptyState =>
        WorkflowState == CalculationWorkflowState.Empty;

    public bool IsLoadingState =>
        WorkflowState == CalculationWorkflowState.Loading;

    public bool IsErrorState =>
        WorkflowState == CalculationWorkflowState.Error;

    public bool IsSuccessState =>
        WorkflowState == CalculationWorkflowState.Success;

    public bool HasResult => IsSuccessState;

    public bool HasCalculationError => IsErrorState;

    public bool IsAwaitingCalculation => IsEmptyState;

    public bool CanEditInputs => !IsLoadingState;

    public bool HasSavedOperatingPoints => SavedOperatingPoints.Count > 0;

    public bool IsOperatingPointComparisonEmpty =>
        !HasSavedOperatingPoints;

    public int SavedOperatingPointCount => SavedOperatingPoints.Count;

    public string SavedOperatingPointCountText =>
        $"{SavedOperatingPointCount} of "
        + $"{MaximumSavedOperatingPointCount} points saved";

    public string SaveOperatingPointButtonText
    {
        get
        {
            if (CurrentResultIsSaved)
            {
                return "Current result saved";
            }

            if (SavedOperatingPointCount
                >= MaximumSavedOperatingPointCount)
            {
                return "Comparison set full";
            }

            return IsSuccessState
                ? "Save current result"
                : "Calculate to save";
        }
    }

    public string ComparisonProtectionNotice =>
        "Saved points are read-only result snapshots. Saving, removing, or "
        + "clearing them never changes the active input form.";

    public string EmptyStateTitle =>
        Inputs.IsInputValid
            ? "Ready to calculate"
            : "Complete the operating point";

    public string EmptyStateMessage =>
        Inputs.IsInputValid
            ? "Inputs are valid. Run the model to see selected-ambient, "
                + "vacuum, and sea-level performance."
            : "Resolve the highlighted input fields before running the model.";

    public string LoadingStateTitle => "Calculating ideal performance";

    public string LoadingStateMessage =>
        "Solving the choked-flow operating point and preparing complete "
        + "performance results.";

    public string CalculateButtonText =>
        WorkflowState switch
        {
            CalculationWorkflowState.Loading => "Calculating…",
            CalculationWorkflowState.Error => "Try calculation again",
            CalculationWorkflowState.Success => "Recalculate performance",
            _ => "Calculate performance",
        };

    public IReadOnlyList<WorkflowStepViewModel> WorkflowSteps { get; }

    private async Task CalculateAsync()
    {
        if (!Inputs.TryCreateInputs(out var inputs) || inputs is null)
        {
            ResetToEmptyState();
            return;
        }

        var calculationRevision = _inputRevision;
        HeadlineResult = null;
        DetailedResult = null;
        NozzleProfiles = null;
        ThrustAltitudePlot = null;
        CalculationErrorMessage = null;
        ClearCurrentComparisonCandidate();
        SetWorkflowState(CalculationWorkflowState.Loading);

        EngineCalculationResult outcome;
        try
        {
            var calculationTask = Task.Run(
                () => _performanceCalculator.Calculate(inputs));
            var minimumDurationTask = Task.Delay(
                _minimumLoadingDuration);
            await Task.WhenAll(
                calculationTask,
                minimumDurationTask);
            outcome = await calculationTask;
        }
        catch (Exception exception)
        {
            if (calculationRevision == _inputRevision)
            {
                PublishError(
                    "The calculation stopped unexpectedly: "
                        + exception.Message);
            }

            return;
        }

        if (calculationRevision != _inputRevision)
        {
            return;
        }

        if (outcome.IsSuccess && outcome.Performance is not null)
        {
            _successfulCalculationId++;
            _currentSuccessfulInputs = inputs;
            _currentSuccessfulPerformance = outcome.Performance;
            DetailedResult = new DetailedPerformanceViewModel(
                outcome.Performance);
            NozzleProfiles = new NozzleFlowProfileViewModel(
                outcome.Performance.NozzleFlowProfile,
                _displayCulture);
            ThrustAltitudePlot = new ThrustAltitudeViewModel(
                outcome.Performance,
                _displayCulture);
            HeadlineResult = new HeadlinePerformanceViewModel(
                inputs.PropellantLabel,
                outcome.Performance);
            SetWorkflowState(CalculationWorkflowState.Success);
            return;
        }

        PublishError(
            outcome.Issues.Count == 0
                ? "The operating point could not be calculated."
                : string.Join(
                    " ",
                    outcome.Issues.Select(static issue => issue.Message)));
    }

    private void PublishError(string message)
    {
        HeadlineResult = null;
        DetailedResult = null;
        NozzleProfiles = null;
        ThrustAltitudePlot = null;
        CalculationErrorMessage = message;
        ClearCurrentComparisonCandidate();
        SetWorkflowState(CalculationWorkflowState.Error);
    }

    private void HandleInputPropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        _inputRevision = unchecked(_inputRevision + 1);
        _calculateCommand.RaiseCanExecuteChanged();
        ResetToEmptyState();
    }

    private void ResetToEmptyState()
    {
        HeadlineResult = null;
        DetailedResult = null;
        NozzleProfiles = null;
        ThrustAltitudePlot = null;
        CalculationErrorMessage = null;
        ClearCurrentComparisonCandidate();
        SetWorkflowState(
            CalculationWorkflowState.Empty,
            notifyWhenUnchanged: true);
    }

    private void SetWorkflowState(
        CalculationWorkflowState workflowState,
        bool notifyWhenUnchanged = false)
    {
        var changed = _workflowState != workflowState;
        if (!changed && !notifyWhenUnchanged)
        {
            return;
        }

        _workflowState = workflowState;
        NozzleDiagram = NozzleStationDiagramViewModel.Create(
            Inputs,
            WorkflowState,
            DetailedResult,
            _displayCulture);
        OnPropertyChanged(nameof(WorkflowState));
        OnPropertyChanged(nameof(NozzleDiagram));
        OnPropertyChanged(nameof(IsEmptyState));
        OnPropertyChanged(nameof(IsLoadingState));
        OnPropertyChanged(nameof(IsErrorState));
        OnPropertyChanged(nameof(IsSuccessState));
        OnPropertyChanged(nameof(HasResult));
        OnPropertyChanged(nameof(HasCalculationError));
        OnPropertyChanged(nameof(IsAwaitingCalculation));
        OnPropertyChanged(nameof(CanEditInputs));
        OnPropertyChanged(nameof(EmptyStateTitle));
        OnPropertyChanged(nameof(EmptyStateMessage));
        OnPropertyChanged(nameof(CalculateButtonText));
        OnPropertyChanged(nameof(StatusText));
        NotifyOperatingPointComparisonState();
        _calculateCommand.RaiseCanExecuteChanged();
    }

    private bool CanSaveCurrentOperatingPoint()
    {
        return IsSuccessState
            && _currentSuccessfulInputs is not null
            && _currentSuccessfulPerformance is not null
            && SavedOperatingPointCount
                < MaximumSavedOperatingPointCount
            && !CurrentResultIsSaved;
    }

    private bool CurrentResultIsSaved =>
        IsSuccessState
        && _currentSuccessfulInputs is not null
        && _currentSuccessfulPerformance is not null
        && _successfulCalculationId > 0
        && SavedOperatingPoints.Any(
            snapshot =>
                snapshot.SourceCalculationId
                == _successfulCalculationId);

    private void SaveCurrentOperatingPoint()
    {
        if (!CanSaveCurrentOperatingPoint())
        {
            return;
        }

        var snapshot = new OperatingPointSnapshotViewModel(
            _nextSnapshotNumber++,
            _successfulCalculationId,
            _currentSuccessfulInputs!,
            _currentSuccessfulPerformance!,
            RemoveOperatingPointSnapshot,
            _displayCulture);
        _savedOperatingPoints.Add(snapshot);
        NotifyOperatingPointComparisonState();
    }

    private void RemoveOperatingPointSnapshot(
        OperatingPointSnapshotViewModel snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        if (_savedOperatingPoints.Remove(snapshot))
        {
            NotifyOperatingPointComparisonState();
        }
    }

    private void ClearOperatingPointComparisons()
    {
        if (_savedOperatingPoints.Count == 0)
        {
            return;
        }

        _savedOperatingPoints.Clear();
        NotifyOperatingPointComparisonState();
    }

    private void ClearCurrentComparisonCandidate()
    {
        _currentSuccessfulInputs = null;
        _currentSuccessfulPerformance = null;
    }

    private void NotifyOperatingPointComparisonState()
    {
        OnPropertyChanged(nameof(HasSavedOperatingPoints));
        OnPropertyChanged(nameof(IsOperatingPointComparisonEmpty));
        OnPropertyChanged(nameof(SavedOperatingPointCount));
        OnPropertyChanged(nameof(SavedOperatingPointCountText));
        OnPropertyChanged(nameof(SaveOperatingPointButtonText));
        _saveCurrentOperatingPointCommand.RaiseCanExecuteChanged();
        _clearOperatingPointComparisonsCommand.RaiseCanExecuteChanged();
    }

    private void OnPropertyChanged(
        [CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}
