using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using LiquidRocketWorkbench.Core.Models;
using LiquidRocketWorkbench.Core.Validation;

namespace LiquidRocketWorkbench.App.ViewModels;

/// <summary>
/// Editable display-unit inputs with Core-backed validation and canonical SI
/// conversion.
/// </summary>
public sealed class EngineInputViewModel
    : INotifyPropertyChanged,
      INotifyDataErrorInfo
{
    private static readonly IReadOnlyDictionary<string, string>
        FieldToProperty = new Dictionary<string, string>
        {
            [EngineInputFields.PropellantLabel] = nameof(PropellantLabel),
            [EngineInputFields.ChamberPressure] =
                nameof(ChamberPressureMegapascals),
            [EngineInputFields.MixtureRatio] = nameof(MixtureRatio),
            [EngineInputFields.ThroatDiameter] =
                nameof(ThroatDiameterMillimeters),
            [EngineInputFields.ExitDiameter] =
                nameof(ExitDiameterMillimeters),
            [EngineInputFields.ChamberTemperature] =
                nameof(ChamberTemperatureKelvin),
            [EngineInputFields.SpecificHeatRatio] =
                nameof(SpecificHeatRatio),
            [EngineInputFields.SpecificGasConstant] =
                nameof(SpecificGasConstantJoulesPerKilogramKelvin),
            [EngineInputFields.AmbientPressure] =
                nameof(AmbientPressureKilopascals),
            [EngineInputFields.TargetMassFlowRate] =
                nameof(TargetMassFlowRateKilogramsPerSecond),
            [EngineInputFields.BurnDuration] =
                nameof(BurnDurationSeconds),
        };

    private readonly EngineInputsValidator _validator;
    private IReadOnlyDictionary<string, IReadOnlyList<string>> _errors =
        new Dictionary<string, IReadOnlyList<string>>();
    private EngineInputs? _currentInputs;
    private ThermodynamicPreset _selectedPreset;
    private bool _isApplyingPreset;
    private string _propellantLabel =
        ThermodynamicPresetCatalog.Default.PropellantLabel;
    private string _chamberPressureMegapascals = "8";
    private string _chamberTemperatureKelvin = "3500";
    private string _specificHeatRatio = "1.22";
    private string _specificGasConstantJoulesPerKilogramKelvin = "355";
    private string _mixtureRatio = "3.5";
    private string _throatDiameterMillimeters = "50";
    private string _exitDiameterMillimeters = "316.22776601683794";
    private string _ambientPressureKilopascals = "101.325";
    private string _targetMassFlowRateKilogramsPerSecond = "20";
    private string _burnDurationSeconds = "10";

    public EngineInputViewModel(EngineInputsValidator validator)
    {
        ArgumentNullException.ThrowIfNull(validator);

        _validator = validator;
        Presets = ThermodynamicPresetCatalog.BuiltIn;
        _selectedPreset = ThermodynamicPresetCatalog.Default;
        ValidateAll();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IReadOnlyList<ThermodynamicPreset> Presets { get; }

    public ThermodynamicPreset SelectedPreset
    {
        get => _selectedPreset;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (Equals(_selectedPreset, value))
            {
                return;
            }

            _selectedPreset = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PresetSourceSummary));

            if (!value.IsCustom)
            {
                ApplyPreset(value);
            }
        }
    }

    public string PresetSourceSummary => SelectedPreset.SourceSummary;

    public string PropellantLabel
    {
        get => _propellantLabel;
        set => SetValue(
            ref _propellantLabel,
            value,
            marksPresetCustom: true);
    }

    public string ChamberPressureMegapascals
    {
        get => _chamberPressureMegapascals;
        set => SetValue(ref _chamberPressureMegapascals, value);
    }

    public string ChamberTemperatureKelvin
    {
        get => _chamberTemperatureKelvin;
        set => SetValue(
            ref _chamberTemperatureKelvin,
            value,
            marksPresetCustom: true);
    }

    public string SpecificHeatRatio
    {
        get => _specificHeatRatio;
        set => SetValue(
            ref _specificHeatRatio,
            value,
            marksPresetCustom: true);
    }

    public string SpecificGasConstantJoulesPerKilogramKelvin
    {
        get => _specificGasConstantJoulesPerKilogramKelvin;
        set => SetValue(
            ref _specificGasConstantJoulesPerKilogramKelvin,
            value,
            marksPresetCustom: true);
    }

    public string MixtureRatio
    {
        get => _mixtureRatio;
        set => SetValue(
            ref _mixtureRatio,
            value,
            marksPresetCustom: true);
    }

    public string ThroatDiameterMillimeters
    {
        get => _throatDiameterMillimeters;
        set => SetValue(ref _throatDiameterMillimeters, value);
    }

    public string ExitDiameterMillimeters
    {
        get => _exitDiameterMillimeters;
        set => SetValue(ref _exitDiameterMillimeters, value);
    }

    public string AmbientPressureKilopascals
    {
        get => _ambientPressureKilopascals;
        set => SetValue(ref _ambientPressureKilopascals, value);
    }

    public string TargetMassFlowRateKilogramsPerSecond
    {
        get => _targetMassFlowRateKilogramsPerSecond;
        set => SetValue(
            ref _targetMassFlowRateKilogramsPerSecond,
            value);
    }

    public string BurnDurationSeconds
    {
        get => _burnDurationSeconds;
        set => SetValue(ref _burnDurationSeconds, value);
    }

    public string this[string propertyName]
    {
        get
        {
            return _errors.TryGetValue(
                propertyName,
                out var propertyErrors)
                ? string.Join(" ", propertyErrors)
                : string.Empty;
        }
    }

    public bool HasErrors => _errors.Count > 0;

    public bool IsInputValid => !HasErrors;

    public string ValidationSummary =>
        HasErrors
            ? "Review the highlighted fields before calculating."
            : "All required inputs are valid.";

    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return _errors.Values.SelectMany(static errors => errors);
        }

        return _errors.TryGetValue(propertyName, out var propertyErrors)
            ? propertyErrors
            : Array.Empty<string>();
    }

    public bool TryCreateInputs(out EngineInputs? inputs)
    {
        ValidateAll();
        inputs = _currentInputs;
        return inputs is not null;
    }

    private void SetValue(
        ref string field,
        string? value,
        bool marksPresetCustom = false,
        [CallerMemberName] string propertyName = "")
    {
        var normalizedValue = value ?? string.Empty;
        if (field == normalizedValue)
        {
            return;
        }

        field = normalizedValue;
        OnPropertyChanged(propertyName);

        if (marksPresetCustom && !_isApplyingPreset)
        {
            MarkPresetCustom();
        }

        ValidateAll();
    }

    private void ApplyPreset(ThermodynamicPreset preset)
    {
        if (preset.IsCustom
            || preset.MixtureRatio is not double mixtureRatio
            || preset.ChamberTemperatureKelvin is not double temperature
            || preset.SpecificHeatRatio is not double specificHeatRatio
            || preset.SpecificGasConstantJoulesPerKilogramKelvin
                is not double gasConstant)
        {
            return;
        }

        _isApplyingPreset = true;
        try
        {
            PropellantLabel = preset.PropellantLabel;
            MixtureRatio = FormatPresetValue(mixtureRatio);
            ChamberTemperatureKelvin = FormatPresetValue(temperature);
            SpecificHeatRatio = FormatPresetValue(specificHeatRatio);
            SpecificGasConstantJoulesPerKilogramKelvin =
                FormatPresetValue(gasConstant);
        }
        finally
        {
            _isApplyingPreset = false;
        }
    }

    private void MarkPresetCustom()
    {
        var customPreset = Presets.Single(
            static preset =>
                preset.Id == ThermodynamicPresetCatalog.CustomId);
        if (Equals(_selectedPreset, customPreset))
        {
            return;
        }

        _selectedPreset = customPreset;
        OnPropertyChanged(nameof(SelectedPreset));
        OnPropertyChanged(nameof(PresetSourceSummary));
    }

    private static string FormatPresetValue(double value)
    {
        return value.ToString("G15", CultureInfo.InvariantCulture);
    }

    private void ValidateAll()
    {
        var newErrors = new Dictionary<string, List<string>>();

        var chamberPressureMegapascals = ParseRequired(
            ChamberPressureMegapascals,
            nameof(ChamberPressureMegapascals),
            "Chamber pressure",
            newErrors);
        var chamberTemperatureKelvin = ParseRequired(
            ChamberTemperatureKelvin,
            nameof(ChamberTemperatureKelvin),
            "Chamber temperature",
            newErrors);
        var specificHeatRatio = ParseRequired(
            SpecificHeatRatio,
            nameof(SpecificHeatRatio),
            "Specific heat ratio",
            newErrors);
        var specificGasConstant = ParseRequired(
            SpecificGasConstantJoulesPerKilogramKelvin,
            nameof(SpecificGasConstantJoulesPerKilogramKelvin),
            "Specific gas constant",
            newErrors);
        var mixtureRatio = ParseRequired(
            MixtureRatio,
            nameof(MixtureRatio),
            "Mixture ratio",
            newErrors);
        var throatDiameterMillimeters = ParseRequired(
            ThroatDiameterMillimeters,
            nameof(ThroatDiameterMillimeters),
            "Throat diameter",
            newErrors);
        var exitDiameterMillimeters = ParseRequired(
            ExitDiameterMillimeters,
            nameof(ExitDiameterMillimeters),
            "Exit diameter",
            newErrors);
        var ambientPressureKilopascals = ParseRequired(
            AmbientPressureKilopascals,
            nameof(AmbientPressureKilopascals),
            "Ambient pressure",
            newErrors);
        var targetMassFlowRate = ParseOptional(
            TargetMassFlowRateKilogramsPerSecond,
            nameof(TargetMassFlowRateKilogramsPerSecond),
            "Target mass flow rate",
            newErrors);
        var burnDuration = ParseOptional(
            BurnDurationSeconds,
            nameof(BurnDurationSeconds),
            "Burn duration",
            newErrors);

        var provisionalInputs = new EngineInputs(
            PropellantLabel,
            ChamberPressurePascals:
                chamberPressureMegapascals * 1_000_000,
            MixtureRatio: mixtureRatio,
            Nozzle: new NozzleGeometry(
                ThroatDiameterMeters:
                    throatDiameterMillimeters / 1000,
                ExitDiameterMeters:
                    exitDiameterMillimeters / 1000),
            Gas: new GasProperties(
                chamberTemperatureKelvin,
                specificHeatRatio,
                specificGasConstant),
            AmbientPressurePascals:
                ambientPressureKilopascals * 1000,
            TargetMassFlowRateKilogramsPerSecond: targetMassFlowRate,
            BurnDurationSeconds: burnDuration);
        var validation = _validator.Validate(provisionalInputs);

        foreach (var issue in validation.Issues)
        {
            if (issue.Field is null
                || !FieldToProperty.TryGetValue(
                    issue.Field,
                    out var propertyName)
                || newErrors.ContainsKey(propertyName))
            {
                continue;
            }

            AddError(newErrors, propertyName, issue.Message);
        }

        ApplyErrors(newErrors);
        _currentInputs = HasErrors ? null : provisionalInputs;
    }

    private static double ParseRequired(
        string text,
        string propertyName,
        string displayName,
        IDictionary<string, List<string>> errors)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            AddError(errors, propertyName, $"Enter {displayName.ToLowerInvariant()}.");
            return double.NaN;
        }

        if (TryParseNumber(text, out var value))
        {
            return value;
        }

        AddError(errors, propertyName, $"{displayName} must be a number.");
        return double.NaN;
    }

    private static double? ParseOptional(
        string text,
        string propertyName,
        string displayName,
        IDictionary<string, List<string>> errors)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        if (TryParseNumber(text, out var value))
        {
            return value;
        }

        AddError(errors, propertyName, $"{displayName} must be a number.");
        return double.NaN;
    }

    private static bool TryParseNumber(string text, out double value)
    {
        const NumberStyles styles = NumberStyles.Float;

        return double.TryParse(
                text,
                styles,
                CultureInfo.CurrentCulture,
                out value)
            || double.TryParse(
                text,
                styles,
                CultureInfo.InvariantCulture,
                out value);
    }

    private static void AddError(
        IDictionary<string, List<string>> errors,
        string propertyName,
        string message)
    {
        if (!errors.TryGetValue(propertyName, out var propertyErrors))
        {
            propertyErrors = [];
            errors[propertyName] = propertyErrors;
        }

        propertyErrors.Add(message);
    }

    private void ApplyErrors(
        IReadOnlyDictionary<string, List<string>> newErrors)
    {
        var normalizedErrors = newErrors.ToDictionary(
            static pair => pair.Key,
            static pair =>
                (IReadOnlyList<string>)Array.AsReadOnly(
                    pair.Value.ToArray()));
        var changedProperties = _errors.Keys
            .Union(normalizedErrors.Keys)
            .Where(
                propertyName =>
                    !SequencesEqual(
                        _errors.GetValueOrDefault(propertyName),
                        normalizedErrors.GetValueOrDefault(propertyName)))
            .ToArray();

        _errors = normalizedErrors;

        foreach (var propertyName in changedProperties)
        {
            ErrorsChanged?.Invoke(
                this,
                new DataErrorsChangedEventArgs(propertyName));
        }

        OnPropertyChanged("Item[]");
        OnPropertyChanged(nameof(HasErrors));
        OnPropertyChanged(nameof(IsInputValid));
        OnPropertyChanged(nameof(ValidationSummary));
    }

    private static bool SequencesEqual(
        IReadOnlyList<string>? left,
        IReadOnlyList<string>? right)
    {
        return left is null
            ? right is null
            : right is not null && left.SequenceEqual(right);
    }

    private void OnPropertyChanged(
        [CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}
