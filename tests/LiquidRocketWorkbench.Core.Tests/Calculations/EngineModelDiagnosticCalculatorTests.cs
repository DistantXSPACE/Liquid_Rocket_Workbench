using LiquidRocketWorkbench.Core.Calculations;
using LiquidRocketWorkbench.Core.Diagnostics;
using LiquidRocketWorkbench.Core.Models;
using LiquidRocketWorkbench.Core.Validation;

namespace LiquidRocketWorkbench.Core.Tests.Calculations;

public sealed class EngineModelDiagnosticCalculatorTests
{
    [Fact]
    public void Evaluate_WithNominalPoint_ReturnsIdealModelWarning()
    {
        var diagnostics = EngineModelDiagnosticCalculator.Evaluate(
            ambientPressurePascals: 100_000,
            nozzleExit: CreateNozzleExit(100_000),
            massFlow: CreateMassFlow());

        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(
            CalculationDiagnosticCodes.IdealFlowAssumptions,
            diagnostic.Code);
        Assert.Equal(ValidationSeverity.Warning, diagnostic.Severity);
        Assert.Null(diagnostic.Field);
    }

    [Fact]
    public void Evaluate_BelowPressureRatioThreshold_AddsModelLimitWarning()
    {
        var diagnostics = EngineModelDiagnosticCalculator.Evaluate(
            ambientPressurePascals: 100_000,
            nozzleExit: CreateNozzleExit(39_999),
            massFlow: CreateMassFlow());

        var warning = Assert.Single(
            diagnostics,
            static issue =>
                issue.Code
                == CalculationDiagnosticCodes
                    .SevereOverexpansionLimit);
        Assert.Equal(ValidationSeverity.Warning, warning.Severity);
        Assert.Contains("does not predict", warning.Message);
        Assert.Null(warning.Field);
    }

    [Theory]
    [InlineData(40_000, 100_000)]
    [InlineData(0, 0)]
    public void Evaluate_AtThresholdOrVacuum_DoesNotAddModelLimitWarning(
        double exitPressure,
        double ambientPressure)
    {
        var diagnostics = EngineModelDiagnosticCalculator.Evaluate(
            ambientPressure,
            CreateNozzleExit(exitPressure),
            CreateMassFlow());

        Assert.DoesNotContain(
            diagnostics,
            static issue =>
                issue.Code
                == CalculationDiagnosticCodes
                    .SevereOverexpansionLimit);
    }

    [Fact]
    public void Evaluate_AboveTargetTolerance_AddsComparisonWarning()
    {
        var diagnostics = EngineModelDiagnosticCalculator.Evaluate(
            ambientPressurePascals: 100_000,
            nozzleExit: CreateNozzleExit(100_000),
            massFlow: CreateMassFlow(relativeTargetDifference: 0.050001));

        var warning = Assert.Single(
            diagnostics,
            static issue =>
                issue.Code
                == CalculationDiagnosticCodes.TargetMassFlowMismatch);
        Assert.Equal(ValidationSeverity.Warning, warning.Severity);
        Assert.Equal(EngineInputFields.TargetMassFlowRate, warning.Field);
    }

    [Fact]
    public void Evaluate_AtTargetTolerance_DoesNotAddComparisonWarning()
    {
        var diagnostics = EngineModelDiagnosticCalculator.Evaluate(
            ambientPressurePascals: 100_000,
            nozzleExit: CreateNozzleExit(100_000),
            massFlow: CreateMassFlow(relativeTargetDifference: 0.05));

        Assert.DoesNotContain(
            diagnostics,
            static issue =>
                issue.Code
                == CalculationDiagnosticCodes.TargetMassFlowMismatch);
    }

    [Fact]
    public void Evaluate_WithAllConditions_ReturnsDeterministicOrder()
    {
        var diagnostics = EngineModelDiagnosticCalculator.Evaluate(
            ambientPressurePascals: 100_000,
            nozzleExit: CreateNozzleExit(39_999),
            massFlow: CreateMassFlow(relativeTargetDifference: 0.06));

        Assert.Equal(
            [
                CalculationDiagnosticCodes.IdealFlowAssumptions,
                CalculationDiagnosticCodes.SevereOverexpansionLimit,
                CalculationDiagnosticCodes.TargetMassFlowMismatch,
            ],
            diagnostics.Select(static issue => issue.Code));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Evaluate_WithInvalidAmbientPressure_Throws(
        double ambientPressure)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => EngineModelDiagnosticCalculator.Evaluate(
                ambientPressure,
                CreateNozzleExit(1),
                CreateMassFlow()));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Evaluate_WithInvalidExitPressure_Throws(double exitPressure)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => EngineModelDiagnosticCalculator.Evaluate(
                ambientPressurePascals: 1,
                nozzleExit: CreateNozzleExit(exitPressure),
                massFlow: CreateMassFlow()));
    }

    [Fact]
    public void Evaluate_WithNullRequiredModel_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => EngineModelDiagnosticCalculator.Evaluate(
                ambientPressurePascals: 1,
                nozzleExit: null!,
                massFlow: CreateMassFlow()));
        Assert.Throws<ArgumentNullException>(
            () => EngineModelDiagnosticCalculator.Evaluate(
                ambientPressurePascals: 1,
                nozzleExit: CreateNozzleExit(1),
                massFlow: null!));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Evaluate_WithInvalidCalculatedMassFlow_Throws(
        double calculatedMassFlow)
    {
        var massFlow = CreateMassFlow() with
        {
            CalculatedMassFlowRateKilogramsPerSecond = calculatedMassFlow,
        };

        Assert.Throws<ArgumentOutOfRangeException>(
            () => EngineModelDiagnosticCalculator.Evaluate(
                ambientPressurePascals: 1,
                nozzleExit: CreateNozzleExit(1),
                massFlow));
    }

    [Fact]
    public void Evaluate_WithIncompleteTargetComparison_Throws()
    {
        var massFlow = CreateMassFlow() with
        {
            TargetMassFlowRateKilogramsPerSecond = 10,
        };

        Assert.Throws<ArgumentException>(
            () => EngineModelDiagnosticCalculator.Evaluate(
                ambientPressurePascals: 1,
                nozzleExit: CreateNozzleExit(1),
                massFlow));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Evaluate_WithInvalidRelativeDifference_Throws(
        double relativeDifference)
    {
        var massFlow = CreateMassFlow(relativeDifference);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => EngineModelDiagnosticCalculator.Evaluate(
                ambientPressurePascals: 1,
                nozzleExit: CreateNozzleExit(1),
                massFlow));
    }

    private static NozzleExitResult CreateNozzleExit(double pressure)
    {
        return new NozzleExitResult(
            MachNumber: 2,
            PressurePascals: pressure,
            TemperatureKelvin: 1000,
            VelocityMetersPerSecond: 2000);
    }

    private static MassFlowResult CreateMassFlow(
        double? relativeTargetDifference = null)
    {
        var hasTarget = relativeTargetDifference.HasValue;

        return new MassFlowResult(
            CalculatedMassFlowRateKilogramsPerSecond: 10,
            OxidizerMassFlowRateKilogramsPerSecond: 7.5,
            FuelMassFlowRateKilogramsPerSecond: 2.5,
            TargetMassFlowRateKilogramsPerSecond: hasTarget ? 10 : null,
            AbsoluteTargetDifferenceKilogramsPerSecond: hasTarget ? 0 : null,
            RelativeTargetDifference: relativeTargetDifference,
            PropellantMassConsumedKilograms: null);
    }
}
