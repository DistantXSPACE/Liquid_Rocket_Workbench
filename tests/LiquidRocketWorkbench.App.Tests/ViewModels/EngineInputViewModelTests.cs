using LiquidRocketWorkbench.App.ViewModels;
using LiquidRocketWorkbench.Core.Validation;

namespace LiquidRocketWorkbench.App.Tests.ViewModels;

public sealed class EngineInputViewModelTests
{
    [Fact]
    public void Constructor_WithNullValidator_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => new EngineInputViewModel(null!));
    }

    [Fact]
    public void Defaults_CreateCanonicalSiInputs()
    {
        var viewModel = CreateViewModel();

        var succeeded = viewModel.TryCreateInputs(out var inputs);

        Assert.True(succeeded);
        Assert.NotNull(inputs);
        Assert.Equal(
            "VC-04 synthetic constant-property gas",
            inputs.PropellantLabel);
        Assert.Equal(8_000_000, inputs.ChamberPressurePascals);
        Assert.Equal(3.5, inputs.MixtureRatio);
        Assert.Equal(0.05, inputs.Nozzle.ThroatDiameterMeters, 12);
        Assert.Equal(
            0.31622776601683794,
            inputs.Nozzle.ExitDiameterMeters,
            12);
        Assert.Equal(3500, inputs.Gas.ChamberTemperatureKelvin);
        Assert.Equal(1.22, inputs.Gas.SpecificHeatRatio);
        Assert.Equal(
            355,
            inputs.Gas.SpecificGasConstantJoulesPerKilogramKelvin);
        Assert.Equal(101_325, inputs.AmbientPressurePascals);
        Assert.Equal(20, inputs.TargetMassFlowRateKilogramsPerSecond);
        Assert.Equal(10, inputs.BurnDurationSeconds);
        Assert.False(viewModel.HasErrors);
        Assert.True(viewModel.IsInputValid);
        Assert.Equal(
            ThermodynamicPresetCatalog.ReferenceCaseId,
            viewModel.SelectedPreset.Id);
        Assert.Contains(
            "VC-04",
            viewModel.PresetSourceSummary,
            StringComparison.Ordinal);
    }

    [Fact]
    public void SelectingPreset_AppliesLabelMixtureAndGasValues()
    {
        var viewModel = CreateViewModel();
        var preset = viewModel.Presets.Single(
            static candidate =>
                candidate.Id
                    == ThermodynamicPresetCatalog.LoxMethaneNasaTestId);

        viewModel.SelectedPreset = preset;

        Assert.True(viewModel.TryCreateInputs(out var inputs));
        Assert.NotNull(inputs);
        Assert.Equal("LOX / Methane", inputs.PropellantLabel);
        Assert.Equal(3.48, inputs.MixtureRatio, 12);
        Assert.Equal(
            3644.44444444444,
            inputs.Gas.ChamberTemperatureKelvin,
            10);
        Assert.Equal(1.182, inputs.Gas.SpecificHeatRatio, 12);
        Assert.Equal(
            402.157968052826,
            inputs.Gas.SpecificGasConstantJoulesPerKilogramKelvin,
            10);
        Assert.Contains(
            "NASA TM-102381",
            viewModel.PresetSourceSummary,
            StringComparison.Ordinal);
    }

    [Fact]
    public void EditingAppliedPresetValue_MarksSelectionCustomAndKeepsEdit()
    {
        var viewModel = CreateViewModel();

        viewModel.SpecificHeatRatio = "1.19";

        Assert.Equal(
            ThermodynamicPresetCatalog.CustomId,
            viewModel.SelectedPreset.Id);
        Assert.Equal("1.19", viewModel.SpecificHeatRatio);
        Assert.Contains(
            "No preset",
            viewModel.PresetSourceSummary,
            StringComparison.Ordinal);
    }

    [Fact]
    public void SelectingCustom_KeepsCurrentEditableValues()
    {
        var viewModel = CreateViewModel();
        var customPreset = viewModel.Presets.Single(
            static preset =>
                preset.Id == ThermodynamicPresetCatalog.CustomId);
        var originalTemperature = viewModel.ChamberTemperatureKelvin;

        viewModel.SelectedPreset = customPreset;

        Assert.Equal(originalTemperature, viewModel.ChamberTemperatureKelvin);
        Assert.Equal(
            "VC-04 synthetic constant-property gas",
            viewModel.PropellantLabel);
    }

    [Fact]
    public void BlankOptionalFields_CreateNullValues()
    {
        var viewModel = CreateViewModel();
        viewModel.TargetMassFlowRateKilogramsPerSecond = string.Empty;
        viewModel.BurnDurationSeconds = " ";

        var succeeded = viewModel.TryCreateInputs(out var inputs);

        Assert.True(succeeded);
        Assert.NotNull(inputs);
        Assert.Null(inputs.TargetMassFlowRateKilogramsPerSecond);
        Assert.Null(inputs.BurnDurationSeconds);
    }

    [Fact]
    public void NonNumericRequiredValue_ShowsInlineErrorAndRejectsInputs()
    {
        var viewModel = CreateViewModel();

        viewModel.ChamberPressureMegapascals = "not a number";

        Assert.True(viewModel.HasErrors);
        Assert.False(viewModel.IsInputValid);
        Assert.Contains(
            "must be a number",
            viewModel[nameof(viewModel.ChamberPressureMegapascals)],
            StringComparison.Ordinal);
        Assert.False(viewModel.TryCreateInputs(out var inputs));
        Assert.Null(inputs);
    }

    [Fact]
    public void InvalidSpecificHeatRatio_UsesCoreValidationMessage()
    {
        var viewModel = CreateViewModel();

        viewModel.SpecificHeatRatio = "1";

        Assert.Contains(
            "greater than one",
            viewModel[nameof(viewModel.SpecificHeatRatio)],
            StringComparison.Ordinal);
    }

    [Fact]
    public void AmbientPressureAtOrAboveChamberPressure_ShowsCrossFieldError()
    {
        var viewModel = CreateViewModel();

        viewModel.ChamberPressureMegapascals = "0.1";

        Assert.Contains(
            "greater than ambient pressure",
            viewModel[nameof(viewModel.ChamberPressureMegapascals)],
            StringComparison.Ordinal);
    }

    [Fact]
    public void ExitDiameterBelowThroatDiameter_ShowsGeometryError()
    {
        var viewModel = CreateViewModel();

        viewModel.ExitDiameterMillimeters = "49";

        Assert.Contains(
            "greater than or equal",
            viewModel[nameof(viewModel.ExitDiameterMillimeters)],
            StringComparison.Ordinal);
    }

    [Fact]
    public void CorrectingInvalidValue_ClearsErrorAndRaisesNotification()
    {
        var viewModel = CreateViewModel();
        var changedProperties = new List<string?>();
        viewModel.ErrorsChanged += (_, args) =>
            changedProperties.Add(args.PropertyName);

        viewModel.MixtureRatio = "invalid";
        viewModel.MixtureRatio = "3.5";

        Assert.Contains(nameof(viewModel.MixtureRatio), changedProperties);
        Assert.Empty(viewModel.GetErrors(nameof(viewModel.MixtureRatio)));
        Assert.True(viewModel.IsInputValid);
    }

    [Fact]
    public void NonNumericOptionalValue_ShowsErrorUntilCleared()
    {
        var viewModel = CreateViewModel();

        viewModel.BurnDurationSeconds = "several";

        Assert.Contains(
            "must be a number",
            viewModel[nameof(viewModel.BurnDurationSeconds)],
            StringComparison.Ordinal);
        Assert.False(viewModel.TryCreateInputs(out _));

        viewModel.BurnDurationSeconds = string.Empty;

        Assert.True(viewModel.TryCreateInputs(out var inputs));
        Assert.NotNull(inputs);
        Assert.Null(inputs.BurnDurationSeconds);
    }

    private static EngineInputViewModel CreateViewModel()
    {
        return new EngineInputViewModel(new EngineInputsValidator());
    }
}
