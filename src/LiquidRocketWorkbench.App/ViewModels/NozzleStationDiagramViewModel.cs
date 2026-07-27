using System.Globalization;

namespace LiquidRocketWorkbench.App.ViewModels;

/// <summary>
/// Immutable annotations for the schematic engine/nozzle station diagram.
/// </summary>
public sealed class NozzleStationDiagramViewModel
{
    private readonly IReadOnlyList<NozzleStationAnnotationViewModel> _stations;

    private NozzleStationDiagramViewModel(
        NozzleStationAnnotationViewModel injector,
        NozzleStationAnnotationViewModel chamber,
        NozzleStationAnnotationViewModel throat,
        NozzleStationAnnotationViewModel exit,
        CalculationWorkflowState workflowState,
        string stateLabel,
        string stateSummary)
    {
        Injector = injector;
        Chamber = chamber;
        Throat = throat;
        Exit = exit;
        WorkflowState = workflowState;
        StateLabel = stateLabel;
        StateSummary = stateSummary;
        _stations = Array.AsReadOnly(
            [Injector, Chamber, Throat, Exit]);
    }

    public NozzleStationAnnotationViewModel Injector { get; }

    public NozzleStationAnnotationViewModel Chamber { get; }

    public NozzleStationAnnotationViewModel Throat { get; }

    public NozzleStationAnnotationViewModel Exit { get; }

    public CalculationWorkflowState WorkflowState { get; }

    public string StateLabel { get; }

    public string StateSummary { get; }

    public bool HasSolvedExit =>
        WorkflowState == CalculationWorkflowState.Success;

    public string ScaleNotice =>
        "Educational flow-path schematic · geometry is not to scale";

    public IReadOnlyList<NozzleStationAnnotationViewModel> Stations => _stations;

    public static NozzleStationDiagramViewModel Create(
        EngineInputViewModel inputs,
        CalculationWorkflowState workflowState,
        DetailedPerformanceViewModel? detailedResult,
        CultureInfo? displayCulture = null)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        var culture = displayCulture ?? CultureInfo.CurrentCulture;

        if (workflowState == CalculationWorkflowState.Success
            && detailedResult is null)
        {
            throw new ArgumentException(
                "A successful diagram requires detailed performance.",
                nameof(detailedResult));
        }

        var injector = new NozzleStationAnnotationViewModel(
            "01",
            "Injector",
            "Propellant entry",
            $"{DisplayText(inputs.PropellantLabel)} · "
                + $"O/F {DisplayText(inputs.MixtureRatio)}");
        var chamber = new NozzleStationAnnotationViewModel(
            "02",
            "Chamber",
            "Stagnation condition",
            $"Pc {DisplayText(inputs.ChamberPressureMegapascals)} MPa · "
                + $"Tc {DisplayText(inputs.ChamberTemperatureKelvin)} K");
        var throat = new NozzleStationAnnotationViewModel(
            "03",
            "Throat",
            "Choked station",
            $"dt {DisplayText(inputs.ThroatDiameterMillimeters)} mm · "
                + "Mach 1 assumed");
        var exitValue = workflowState == CalculationWorkflowState.Success
            ? $"de {DisplayText(inputs.ExitDiameterMillimeters)} mm · "
                + $"Mach {Format(
                    detailedResult!.ExitMachNumber,
                    "N3",
                    culture)}"
            : $"de {DisplayText(inputs.ExitDiameterMillimeters)} mm · "
                + "solution pending";
        var exit = new NozzleStationAnnotationViewModel(
            "04",
            "Exit",
            "Supersonic plane",
            exitValue);

        var (stateLabel, stateSummary) = workflowState switch
        {
            CalculationWorkflowState.Loading => (
                "SOLVING",
                "Calculating the choked-flow and exit station annotations."),
            CalculationWorkflowState.Error => (
                "SOLUTION UNAVAILABLE",
                "The schematic still reflects current inputs; review the "
                    + "calculation error before using exit values."),
            CalculationWorkflowState.Success => (
                "SOLUTION ANNOTATED",
                $"Exit: Pe {Format(
                    detailedResult!.ExitPressureKilopascals,
                    "N3",
                    culture)} kPa · "
                    + $"Te {Format(
                        detailedResult.ExitTemperatureKelvin,
                        "N1",
                        culture)} K · "
                    + $"Ve {Format(
                        detailedResult.ExitVelocityMetersPerSecond,
                        "N1",
                        culture)} m/s"),
            _ => (
                "INPUT SCHEMATIC",
                "Calculate to annotate the exit plane with the current ideal "
                    + "solution."),
        };

        return new NozzleStationDiagramViewModel(
            injector,
            chamber,
            throat,
            exit,
            workflowState,
            stateLabel,
            stateSummary);
    }

    private static string DisplayText(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "—"
            : value.Trim();
    }

    private static string Format(
        double value,
        string format,
        CultureInfo culture)
    {
        return value.ToString(format, culture);
    }
}
