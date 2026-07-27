using LiquidRocketWorkbench.Core.Diagnostics;
using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.Core.Validation;

/// <summary>
/// Validates an ideal-engine operating point without performing calculations.
/// </summary>
public sealed class EngineInputsValidator
{
    public ValidationResult Validate(EngineInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var issues = new List<ValidationIssue>();

        ValidatePropellantLabel(inputs.PropellantLabel, issues);

        var chamberPressureIsValid = ValidatePositive(
            inputs.ChamberPressurePascals,
            EngineInputFields.ChamberPressure,
            "Chamber pressure",
            issues);
        ValidatePositive(
            inputs.MixtureRatio,
            EngineInputFields.MixtureRatio,
            "Mixture ratio",
            issues);

        ValidateNozzle(inputs.Nozzle, issues);
        ValidateGas(inputs.Gas, issues);

        var ambientPressureIsValid = ValidateNonnegative(
            inputs.AmbientPressurePascals,
            EngineInputFields.AmbientPressure,
            "Ambient pressure",
            issues);

        if (inputs.TargetMassFlowRateKilogramsPerSecond is double targetMassFlow)
        {
            ValidatePositive(
                targetMassFlow,
                EngineInputFields.TargetMassFlowRate,
                "Target mass flow rate",
                issues);
        }

        if (inputs.BurnDurationSeconds is double burnDuration)
        {
            ValidatePositive(
                burnDuration,
                EngineInputFields.BurnDuration,
                "Burn duration",
                issues);
        }

        if (chamberPressureIsValid
            && ambientPressureIsValid
            && inputs.ChamberPressurePascals <= inputs.AmbientPressurePascals)
        {
            issues.Add(
                new ValidationIssue(
                    EngineInputValidationCodes
                        .ChamberPressureNotAboveAmbient,
                    ValidationSeverity.Error,
                    "Chamber pressure must be greater than ambient pressure.",
                    EngineInputFields.ChamberPressure));
        }

        return new ValidationResult(issues);
    }

    private static void ValidatePropellantLabel(
        string propellantLabel,
        ICollection<ValidationIssue> issues)
    {
        if (!string.IsNullOrWhiteSpace(propellantLabel))
        {
            return;
        }

        issues.Add(
            new ValidationIssue(
                EngineInputValidationCodes.PropellantLabelRequired,
                ValidationSeverity.Error,
                "Enter a propellant label or use \"Custom\".",
                EngineInputFields.PropellantLabel));
    }

    private static void ValidateNozzle(
        NozzleGeometry nozzle,
        ICollection<ValidationIssue> issues)
    {
        if (nozzle is null)
        {
            issues.Add(
                new ValidationIssue(
                    EngineInputValidationCodes.NozzleRequired,
                    ValidationSeverity.Error,
                    "Nozzle geometry is required.",
                    EngineInputFields.Nozzle));
            return;
        }

        var throatIsValid = ValidatePositive(
            nozzle.ThroatDiameterMeters,
            EngineInputFields.ThroatDiameter,
            "Throat diameter",
            issues);
        var exitIsValid = ValidatePositive(
            nozzle.ExitDiameterMeters,
            EngineInputFields.ExitDiameter,
            "Exit diameter",
            issues);

        if (throatIsValid
            && exitIsValid
            && nozzle.ExitDiameterMeters < nozzle.ThroatDiameterMeters)
        {
            issues.Add(
                new ValidationIssue(
                    EngineInputValidationCodes
                        .ExitDiameterSmallerThanThroat,
                    ValidationSeverity.Error,
                    "Exit diameter must be greater than or equal to throat "
                        + "diameter.",
                    EngineInputFields.ExitDiameter));
        }
    }

    private static void ValidateGas(
        GasProperties gas,
        ICollection<ValidationIssue> issues)
    {
        if (gas is null)
        {
            issues.Add(
                new ValidationIssue(
                    EngineInputValidationCodes.GasRequired,
                    ValidationSeverity.Error,
                    "Gas properties are required.",
                    EngineInputFields.Gas));
            return;
        }

        ValidatePositive(
            gas.ChamberTemperatureKelvin,
            EngineInputFields.ChamberTemperature,
            "Chamber temperature",
            issues);
        ValidateSpecificHeatRatio(gas.SpecificHeatRatio, issues);
        ValidatePositive(
            gas.SpecificGasConstantJoulesPerKilogramKelvin,
            EngineInputFields.SpecificGasConstant,
            "Specific gas constant",
            issues);
    }

    private static void ValidateSpecificHeatRatio(
        double specificHeatRatio,
        ICollection<ValidationIssue> issues)
    {
        if (!double.IsFinite(specificHeatRatio))
        {
            AddNotFiniteIssue(
                EngineInputFields.SpecificHeatRatio,
                "Specific heat ratio",
                issues);
            return;
        }

        if (specificHeatRatio > 1)
        {
            return;
        }

        issues.Add(
            new ValidationIssue(
                EngineInputValidationCodes.SpecificHeatRatioMustExceedOne,
                ValidationSeverity.Error,
                "Specific heat ratio must be greater than one.",
                EngineInputFields.SpecificHeatRatio));
    }

    private static bool ValidatePositive(
        double value,
        string field,
        string displayName,
        ICollection<ValidationIssue> issues)
    {
        if (!double.IsFinite(value))
        {
            AddNotFiniteIssue(field, displayName, issues);
            return false;
        }

        if (value > 0)
        {
            return true;
        }

        issues.Add(
            new ValidationIssue(
                EngineInputValidationCodes.MustBePositive,
                ValidationSeverity.Error,
                $"{displayName} must be greater than zero.",
                field));
        return false;
    }

    private static bool ValidateNonnegative(
        double value,
        string field,
        string displayName,
        ICollection<ValidationIssue> issues)
    {
        if (!double.IsFinite(value))
        {
            AddNotFiniteIssue(field, displayName, issues);
            return false;
        }

        if (value >= 0)
        {
            return true;
        }

        issues.Add(
            new ValidationIssue(
                EngineInputValidationCodes.MustBeNonnegative,
                ValidationSeverity.Error,
                $"{displayName} cannot be negative.",
                field));
        return false;
    }

    private static void AddNotFiniteIssue(
        string field,
        string displayName,
        ICollection<ValidationIssue> issues)
    {
        issues.Add(
            new ValidationIssue(
                EngineInputValidationCodes.NotFinite,
                ValidationSeverity.Error,
                $"{displayName} must be a finite number.",
                field));
    }
}
