using LiquidRocketWorkbench.Core.Diagnostics;
using LiquidRocketWorkbench.Core.Models;
using LiquidRocketWorkbench.Core.Validation;

namespace LiquidRocketWorkbench.Core.Tests.Validation;

public sealed class EngineInputsValidatorTests
{
    private readonly EngineInputsValidator _validator = new();

    [Fact]
    public void Validate_WithValidOperatingPoint_ReturnsNoIssues()
    {
        var result = _validator.Validate(CreateValidInputs());

        Assert.True(result.IsValid);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Validate_WithInvalidFields_ReturnsOneIssueForEveryField()
    {
        var inputs = new EngineInputs(
            PropellantLabel: " ",
            ChamberPressurePascals: double.NaN,
            MixtureRatio: 0,
            Nozzle: new NozzleGeometry(
                ThroatDiameterMeters: -0.01,
                ExitDiameterMeters: double.PositiveInfinity),
            Gas: new GasProperties(
                ChamberTemperatureKelvin: 0,
                SpecificHeatRatio: 1,
                SpecificGasConstantJoulesPerKilogramKelvin: double.NaN),
            AmbientPressurePascals: -1,
            TargetMassFlowRateKilogramsPerSecond: double.NegativeInfinity,
            BurnDurationSeconds: 0);

        var result = _validator.Validate(inputs);

        Assert.False(result.IsValid);
        Assert.Equal(11, result.Issues.Count);
        AssertIssue(
            result,
            EngineInputFields.PropellantLabel,
            EngineInputValidationCodes.PropellantLabelRequired);
        AssertIssue(
            result,
            EngineInputFields.ChamberPressure,
            EngineInputValidationCodes.NotFinite);
        AssertIssue(
            result,
            EngineInputFields.MixtureRatio,
            EngineInputValidationCodes.MustBePositive);
        AssertIssue(
            result,
            EngineInputFields.ThroatDiameter,
            EngineInputValidationCodes.MustBePositive);
        AssertIssue(
            result,
            EngineInputFields.ExitDiameter,
            EngineInputValidationCodes.NotFinite);
        AssertIssue(
            result,
            EngineInputFields.ChamberTemperature,
            EngineInputValidationCodes.MustBePositive);
        AssertIssue(
            result,
            EngineInputFields.SpecificHeatRatio,
            EngineInputValidationCodes.SpecificHeatRatioMustExceedOne);
        AssertIssue(
            result,
            EngineInputFields.SpecificGasConstant,
            EngineInputValidationCodes.NotFinite);
        AssertIssue(
            result,
            EngineInputFields.AmbientPressure,
            EngineInputValidationCodes.MustBeNonnegative);
        AssertIssue(
            result,
            EngineInputFields.TargetMassFlowRate,
            EngineInputValidationCodes.NotFinite);
        AssertIssue(
            result,
            EngineInputFields.BurnDuration,
            EngineInputValidationCodes.MustBePositive);
    }

    [Fact]
    public void Validate_WithInvalidCrossFieldValues_ReturnsActionableIssues()
    {
        var inputs = CreateValidInputs() with
        {
            ChamberPressurePascals = 100_000,
            AmbientPressurePascals = 100_000,
            Nozzle = new NozzleGeometry(
                ThroatDiameterMeters: 0.05,
                ExitDiameterMeters: 0.04),
        };

        var result = _validator.Validate(inputs);

        Assert.False(result.IsValid);
        Assert.Equal(2, result.Issues.Count);
        AssertIssue(
            result,
            EngineInputFields.ExitDiameter,
            EngineInputValidationCodes.ExitDiameterSmallerThanThroat);
        AssertIssue(
            result,
            EngineInputFields.ChamberPressure,
            EngineInputValidationCodes.ChamberPressureNotAboveAmbient);
    }

    [Fact]
    public void Validate_AtAllowedBoundaries_ReturnsNoIssues()
    {
        var inputs = CreateValidInputs() with
        {
            ChamberPressurePascals = 101_326,
            AmbientPressurePascals = 101_325,
            Nozzle = new NozzleGeometry(
                ThroatDiameterMeters: 0.05,
                ExitDiameterMeters: 0.05),
            Gas = CreateValidInputs().Gas with
            {
                SpecificHeatRatio = double.BitIncrement(1),
            },
            TargetMassFlowRateKilogramsPerSecond = null,
            BurnDurationSeconds = null,
        };

        var result = _validator.Validate(inputs);

        Assert.True(result.IsValid);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Validate_WithMissingNestedModels_ReturnsIssuesWithoutThrowing()
    {
        var inputs = CreateValidInputs() with
        {
            Nozzle = null!,
            Gas = null!,
        };

        var result = _validator.Validate(inputs);

        Assert.False(result.IsValid);
        Assert.Equal(2, result.Issues.Count);
        AssertIssue(
            result,
            EngineInputFields.Nozzle,
            EngineInputValidationCodes.NozzleRequired);
        AssertIssue(
            result,
            EngineInputFields.Gas,
            EngineInputValidationCodes.GasRequired);
    }

    [Fact]
    public void Validate_WithNullOperatingPoint_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _validator.Validate(null!));
    }

    private static EngineInputs CreateValidInputs()
    {
        return new EngineInputs(
            PropellantLabel: "Custom",
            ChamberPressurePascals: 8_000_000,
            MixtureRatio: 3.5,
            Nozzle: new NozzleGeometry(
                ThroatDiameterMeters: 0.05,
                ExitDiameterMeters: 0.31622776601683794),
            Gas: new GasProperties(
                ChamberTemperatureKelvin: 3500,
                SpecificHeatRatio: 1.22,
                SpecificGasConstantJoulesPerKilogramKelvin: 355),
            AmbientPressurePascals: 101_325,
            TargetMassFlowRateKilogramsPerSecond: 20,
            BurnDurationSeconds: 10);
    }

    private static void AssertIssue(
        ValidationResult result,
        string field,
        string code)
    {
        Assert.Contains(
            result.Issues,
            issue => issue.Field == field && issue.Code == code);
    }
}
