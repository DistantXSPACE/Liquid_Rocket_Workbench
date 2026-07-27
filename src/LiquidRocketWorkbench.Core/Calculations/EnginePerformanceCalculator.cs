using LiquidRocketWorkbench.Core.Diagnostics;
using LiquidRocketWorkbench.Core.Models;
using LiquidRocketWorkbench.Core.Validation;

namespace LiquidRocketWorkbench.Core.Calculations;

/// <summary>
/// Composes the documented Phase 1 ideal-engine calculation chain.
/// </summary>
public sealed class EnginePerformanceCalculator
    : IEnginePerformanceCalculator
{
    private readonly EngineInputsValidator _inputsValidator = new();

    public EngineCalculationResult Calculate(EngineInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var validation = _inputsValidator.Validate(inputs);
        if (!validation.IsValid)
        {
            return EngineCalculationResult.Failed(validation.Issues);
        }

        try
        {
            return CalculateValidated(inputs);
        }
        catch (ArithmeticException exception)
        {
            return EngineCalculationResult.Failed(
                [
                    new ValidationIssue(
                        CalculationDiagnosticCodes.NumericFailure,
                        ValidationSeverity.Error,
                        "The operating point could not be represented "
                            + "numerically. Reduce extreme input magnitudes "
                            + $"and try again. {exception.Message}"),
                ]);
        }
    }

    private static EngineCalculationResult CalculateValidated(
        EngineInputs inputs)
    {
        var geometry = NozzleGeometryCalculator.Calculate(inputs.Nozzle);
        var machSolution = SupersonicAreaMachSolver.Solve(
            geometry.ExpansionRatio,
            inputs.Gas.SpecificHeatRatio);

        if (!machSolution.IsConverged)
        {
            return EngineCalculationResult.Failed(
                [machSolution.Diagnostic!]);
        }

        var massFlow = MassFlowCalculator.Calculate(
            inputs.ChamberPressurePascals,
            geometry.ThroatAreaSquareMeters,
            inputs.Gas,
            inputs.MixtureRatio,
            inputs.TargetMassFlowRateKilogramsPerSecond,
            inputs.BurnDurationSeconds);
        var nozzleExit = NozzleExitCalculator.Calculate(
            machSolution.MachNumber!.Value,
            inputs.ChamberPressurePascals,
            inputs.Gas);
        var nozzleFlowProfile = NozzleFlowProfileCalculator.Calculate(
            nozzleExit.MachNumber,
            inputs.ChamberPressurePascals,
            inputs.Gas);
        var selectedAmbientPerformance =
            AmbientPerformanceCalculator.Calculate(
                inputs.AmbientPressurePascals,
                inputs.ChamberPressurePascals,
                massFlow.CalculatedMassFlowRateKilogramsPerSecond,
                geometry,
                nozzleExit);
        var vacuumPerformance = AmbientPerformanceCalculator.Calculate(
            ambientPressurePascals: 0,
            inputs.ChamberPressurePascals,
            massFlow.CalculatedMassFlowRateKilogramsPerSecond,
            geometry,
            nozzleExit);
        var seaLevelPerformance = AmbientPerformanceCalculator.Calculate(
            PhysicalConstants.StandardSeaLevelPressurePascals,
            inputs.ChamberPressurePascals,
            massFlow.CalculatedMassFlowRateKilogramsPerSecond,
            geometry,
            nozzleExit);
        var thrustAltitudeProfile =
            StandardAtmosphereThrustProfileCalculator.Calculate(
                inputs.AmbientPressurePascals,
                inputs.ChamberPressurePascals,
                massFlow.CalculatedMassFlowRateKilogramsPerSecond,
                geometry,
                nozzleExit);
        var characteristicVelocity =
            CharacteristicVelocityCalculator.CalculateIdeal(inputs.Gas);
        var diagnostics = EngineModelDiagnosticCalculator.Evaluate(
            inputs.AmbientPressurePascals,
            nozzleExit,
            massFlow);
        var performance = new EnginePerformanceResult(
            geometry,
            massFlow,
            nozzleExit,
            nozzleFlowProfile,
            thrustAltitudeProfile,
            selectedAmbientPerformance,
            vacuumPerformance,
            seaLevelPerformance,
            characteristicVelocity,
            diagnostics);

        return EngineCalculationResult.Succeeded(performance);
    }
}
