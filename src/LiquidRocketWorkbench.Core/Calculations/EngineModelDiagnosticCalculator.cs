using LiquidRocketWorkbench.Core.Diagnostics;
using LiquidRocketWorkbench.Core.Models;
using LiquidRocketWorkbench.Core.Validation;

namespace LiquidRocketWorkbench.Core.Calculations;

/// <summary>
/// Produces non-fatal diagnostics for the selected ideal-engine operating
/// point.
/// </summary>
public static class EngineModelDiagnosticCalculator
{
    public static IReadOnlyList<ValidationIssue> Evaluate(
        double ambientPressurePascals,
        NozzleExitResult nozzleExit,
        MassFlowResult massFlow)
    {
        ArgumentNullException.ThrowIfNull(nozzleExit);
        ArgumentNullException.ThrowIfNull(massFlow);
        CalculationGuard.RequireNonnegativeFinite(
            ambientPressurePascals,
            nameof(ambientPressurePascals));
        CalculationGuard.RequireNonnegativeFinite(
            nozzleExit.PressurePascals,
            nameof(nozzleExit.PressurePascals));
        ValidateMassFlow(massFlow);

        var diagnostics = new List<ValidationIssue>
        {
            new(
                CalculationDiagnosticCodes.IdealFlowAssumptions,
                ValidationSeverity.Warning,
                "Results use a one-dimensional, steady, adiabatic, "
                    + "isentropic ideal-gas model with constant gas "
                    + "properties; combustion chemistry and nozzle losses "
                    + "are not modeled."),
        };

        if (ambientPressurePascals > 0
            && nozzleExit.PressurePascals / ambientPressurePascals
                < ModelWarningThresholds
                    .MinimumExitToAmbientPressureRatio)
        {
            diagnostics.Add(
                new ValidationIssue(
                    CalculationDiagnosticCodes
                        .SevereOverexpansionLimit,
                    ValidationSeverity.Warning,
                    "Exit pressure is below 40% of ambient pressure. The "
                        + "ideal isentropic model does not predict shocks, "
                        + "flow separation, or side loads; treat this "
                        + "operating point cautiously."));
        }

        if (massFlow.RelativeTargetDifference is double relativeDifference
            && relativeDifference
                > CalculationTolerances.TargetMassFlowRelative)
        {
            diagnostics.Add(
                new ValidationIssue(
                    CalculationDiagnosticCodes.TargetMassFlowMismatch,
                    ValidationSeverity.Warning,
                    "Target mass flow differs from the geometry-driven ideal "
                        + "flow by more than 5%. Treat the target as "
                        + "comparison-only and review the inputs.",
                    EngineInputFields.TargetMassFlowRate));
        }

        return Array.AsReadOnly(diagnostics.ToArray());
    }

    private static void ValidateMassFlow(MassFlowResult massFlow)
    {
        CalculationGuard.RequirePositiveFinite(
            massFlow.CalculatedMassFlowRateKilogramsPerSecond,
            nameof(massFlow.CalculatedMassFlowRateKilogramsPerSecond));

        var hasTarget =
            massFlow.TargetMassFlowRateKilogramsPerSecond.HasValue;
        var hasAbsoluteDifference =
            massFlow.AbsoluteTargetDifferenceKilogramsPerSecond.HasValue;
        var hasRelativeDifference =
            massFlow.RelativeTargetDifference.HasValue;

        if (hasTarget != hasAbsoluteDifference
            || hasTarget != hasRelativeDifference)
        {
            throw new ArgumentException(
                "Target mass flow and both comparison differences must be "
                    + "present together or all omitted.",
                nameof(massFlow));
        }

        if (!hasTarget)
        {
            return;
        }

        CalculationGuard.RequirePositiveFinite(
            massFlow.TargetMassFlowRateKilogramsPerSecond!.Value,
            nameof(massFlow.TargetMassFlowRateKilogramsPerSecond));
        CalculationGuard.RequireNonnegativeFinite(
            massFlow.AbsoluteTargetDifferenceKilogramsPerSecond!.Value,
            nameof(massFlow.AbsoluteTargetDifferenceKilogramsPerSecond));
        CalculationGuard.RequireNonnegativeFinite(
            massFlow.RelativeTargetDifference!.Value,
            nameof(massFlow.RelativeTargetDifference));
    }
}
