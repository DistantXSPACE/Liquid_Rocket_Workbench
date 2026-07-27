using LiquidRocketWorkbench.Core.Calculations;
using LiquidRocketWorkbench.Core.Models;
using LiquidRocketWorkbench.Core.Validation;

namespace LiquidRocketWorkbench.Core.Tests.Calculations;

public sealed class EnginePerformanceCalculatorTests
{
    private readonly IEnginePerformanceCalculator _calculator =
        new EnginePerformanceCalculator();

    [Fact]
    public void Calculate_WithVc02Inputs_ComposesIsentropicReferencePoint()
    {
        var inputs = new EngineInputs(
            PropellantLabel: "VC-02 synthetic gas",
            ChamberPressurePascals: 1,
            MixtureRatio: 1,
            Nozzle: new NozzleGeometry(
                ThroatDiameterMeters: 1,
                ExitDiameterMeters: 1.299038105676658),
            Gas: new GasProperties(
                ChamberTemperatureKelvin: 1,
                SpecificHeatRatio: 1.4,
                SpecificGasConstantJoulesPerKilogramKelvin: 1),
            AmbientPressurePascals: 0);

        var outcome = _calculator.Calculate(inputs);

        var result = AssertSuccess(outcome);
        AssertRelative(expected: 1.6875, result.Geometry.ExpansionRatio);
        AssertRelative(expected: 2, result.NozzleExit.MachNumber);
        AssertRelative(
            expected: 0.5555555555555556,
            result.NozzleExit.TemperatureKelvin);
        AssertRelative(
            expected: 0.12780452546295096,
            result.NozzleExit.PressurePascals);

        var densityRatio =
            result.NozzleExit.PressurePascals
            / result.NozzleExit.TemperatureKelvin;
        AssertRelative(
            expected: 0.23004814583331168,
            densityRatio);
    }

    [Fact]
    public void Calculate_WithVc03Inputs_ComposesChokedFlowAndCstarIdentity()
    {
        var inputs = new EngineInputs(
            PropellantLabel: "VC-03 synthetic gas",
            ChamberPressurePascals: 1_000_000,
            MixtureRatio: 1,
            Nozzle: new NozzleGeometry(
                ThroatDiameterMeters: 0.11283791670955126,
                ExitDiameterMeters: 0.11283791670955126),
            Gas: new GasProperties(
                ChamberTemperatureKelvin: 300,
                SpecificHeatRatio: 1.4,
                SpecificGasConstantJoulesPerKilogramKelvin: 287.05),
            AmbientPressurePascals: 0);

        var outcome = _calculator.Calculate(inputs);

        var result = AssertSuccess(outcome);
        AssertRelative(
            expected: 0.01,
            result.Geometry.ThroatAreaSquareMeters);
        AssertRelative(
            expected: 23.333553155106348,
            result.MassFlow.CalculatedMassFlowRateKilogramsPerSecond);
        AssertRelative(
            expected: 428.56739106669596,
            result.CharacteristicVelocityMetersPerSecond);

        var identityCstar =
            inputs.ChamberPressurePascals
            * result.Geometry.ThroatAreaSquareMeters
            / result.MassFlow.CalculatedMassFlowRateKilogramsPerSecond;
        AssertRelative(
            expected: 428.56739106669596,
            identityCstar);
    }

    [Fact]
    public void Calculate_WithVc04Inputs_MatchesCompleteFixedReferenceChain()
    {
        var outcome = _calculator.Calculate(CreateVc04Inputs());

        var result = AssertSuccess(outcome);
        AssertRelative(
            expected: 0.001963495408493621,
            result.Geometry.ThroatAreaSquareMeters);
        AssertRelative(
            expected: 0.07853981633974483,
            result.Geometry.ExitAreaSquareMeters);
        AssertRelative(expected: 40, result.Geometry.ExpansionRatio);
        AssertRelative(
            expected: 4.355537019605814,
            result.NozzleExit.MachNumber,
            tolerance: 1e-6);
        AssertRelative(
            expected: 1133.8686466837182,
            result.NozzleExit.TemperatureKelvin,
            tolerance: 1e-6);
        AssertRelative(
            expected: 15_436.918385488754,
            result.NozzleExit.PressurePascals,
            tolerance: 1e-6);
        AssertRelative(
            expected: 3052.229422333306,
            result.NozzleExit.VelocityMetersPerSecond,
            tolerance: 1e-6);
        Assert.Equal(
            NozzleFlowProfileCalculator.ProfilePointCount,
            result.NozzleFlowProfile.Points.Count);
        Assert.Equal(0, result.NozzleFlowProfile.Chamber.MachNumber);
        Assert.Equal(
            8_000_000,
            result.NozzleFlowProfile.Chamber.PressurePascals);
        Assert.Equal(1, result.NozzleFlowProfile.Throat.MachNumber);
        AssertRelative(
            result.NozzleExit.MachNumber,
            result.NozzleFlowProfile.Exit.MachNumber,
            tolerance: 1e-12);
        AssertRelative(
            result.NozzleExit.PressurePascals,
            result.NozzleFlowProfile.Exit.PressurePascals,
            tolerance: 1e-12);
        AssertRelative(
            result.NozzleExit.TemperatureKelvin,
            result.NozzleFlowProfile.Exit.TemperatureKelvin,
            tolerance: 1e-12);
        Assert.Equal(
            StandardAtmosphereThrustProfileCalculator.ProfilePointCount,
            result.ThrustAltitudeProfile.Points.Count);
        AssertRelative(
            result.SeaLevelPerformance.TotalThrustNewtons,
            result.ThrustAltitudeProfile.SeaLevel.TotalThrustNewtons,
            tolerance: 1e-12);
        Assert.Equal(
            0,
            result.ThrustAltitudeProfile
                .SelectedAmbientEquivalentGeopotentialAltitudeMeters);
        AssertRelative(
            expected: 9.193408634242926,
            result.MassFlow.CalculatedMassFlowRateKilogramsPerSecond);
        AssertAmbientPerformance(
            result.VacuumPerformance,
            expectedPressureThrust: 1212.412734847917,
            expectedTotalThrust: 29_272.80505981723,
            expectedSpecificImpulse: 324.6886449456306,
            expectedThrustCoefficient: 1.8635646493741427,
            expectedState: NozzleExpansionState.Underexpanded);
        AssertAmbientPerformance(
            result.SeaLevelPerformance,
            expectedPressureThrust: -6745.634155776728,
            expectedTotalThrust: 21_314.758169192588,
            expectedSpecificImpulse: 236.41943206867248,
            expectedThrustCoefficient: 1.3569396493741428,
            expectedState: NozzleExpansionState.Overexpanded);
        AssertAmbientPerformance(
            result.SelectedAmbientPerformance,
            expectedPressureThrust: -6745.634155776728,
            expectedTotalThrust: 21_314.758169192588,
            expectedSpecificImpulse: 236.41943206867248,
            expectedThrustCoefficient: 1.3569396493741428,
            expectedState: NozzleExpansionState.Overexpanded);
        AssertRelative(
            expected: 1708.6114511913577,
            result.CharacteristicVelocityMetersPerSecond);
        Assert.Equal(
            [
                CalculationDiagnosticCodes.IdealFlowAssumptions,
                CalculationDiagnosticCodes.SevereOverexpansionLimit,
                CalculationDiagnosticCodes.TargetMassFlowMismatch,
            ],
            result.Diagnostics.Select(static issue => issue.Code));
        Assert.Equal(
            result.Diagnostics.Select(static issue => issue.Code),
            outcome.Issues.Select(static issue => issue.Code));
    }

    [Theory]
    [InlineData(101_900, NozzleExpansionState.IdeallyExpanded)]
    [InlineData(102_100, NozzleExpansionState.Overexpanded)]
    [InlineData(97_000, NozzleExpansionState.Underexpanded)]
    public void Calculate_WithComposedVc05Case_ReturnsExpectedState(
        double ambientPressure,
        NozzleExpansionState expectedState)
    {
        var inputs = new EngineInputs(
            PropellantLabel: "VC-05 synthetic gas",
            ChamberPressurePascals: 782_444.9066867264,
            MixtureRatio: 1,
            Nozzle: new NozzleGeometry(
                ThroatDiameterMeters: 0.01,
                ExitDiameterMeters: 0.01299038105676658),
            Gas: new GasProperties(
                ChamberTemperatureKelvin: 300,
                SpecificHeatRatio: 1.4,
                SpecificGasConstantJoulesPerKilogramKelvin: 287.05),
            AmbientPressurePascals: ambientPressure);

        var outcome = _calculator.Calculate(inputs);

        var result = AssertSuccess(outcome);
        AssertRelative(
            expected: 100_000,
            result.NozzleExit.PressurePascals,
            tolerance: 1e-6);
        Assert.Equal(
            expectedState,
            result.SelectedAmbientPerformance.NozzleExpansionState);
    }

    [Fact]
    public void Calculate_WithInvalidInputs_ReturnsValidationFailure()
    {
        var inputs = CreateVc04Inputs() with
        {
            PropellantLabel = string.Empty,
        };

        var outcome = _calculator.Calculate(inputs);

        Assert.False(outcome.IsSuccess);
        Assert.Null(outcome.Performance);
        Assert.Contains(
            outcome.Issues,
            static issue =>
                issue.Code
                == EngineInputValidationCodes.PropellantLabelRequired);
    }

    [Fact]
    public void Calculate_WhenMachRootIsNotBracketed_ReturnsSolverFailure()
    {
        var inputs = new EngineInputs(
            PropellantLabel: "Unbracketed",
            ChamberPressurePascals: 1_000_000,
            MixtureRatio: 1,
            Nozzle: new NozzleGeometry(
                ThroatDiameterMeters: 1e-6,
                ExitDiameterMeters: 1e6),
            Gas: new GasProperties(
                ChamberTemperatureKelvin: 300,
                SpecificHeatRatio: 1.4,
                SpecificGasConstantJoulesPerKilogramKelvin: 287.05),
            AmbientPressurePascals: 0);

        var outcome = _calculator.Calculate(inputs);

        Assert.False(outcome.IsSuccess);
        Assert.Null(outcome.Performance);
        Assert.Equal(
            CalculationDiagnosticCodes.AreaMachRootNotBracketed,
            Assert.Single(outcome.Issues).Code);
    }

    [Fact]
    public void Calculate_WhenValidMagnitudesUnderflow_ReturnsNumericFailure()
    {
        var inputs = new EngineInputs(
            PropellantLabel: "Underflow",
            ChamberPressurePascals: 1,
            MixtureRatio: 1,
            Nozzle: new NozzleGeometry(
                ThroatDiameterMeters: double.Epsilon,
                ExitDiameterMeters: double.Epsilon),
            Gas: new GasProperties(
                ChamberTemperatureKelvin: 1,
                SpecificHeatRatio: 1.4,
                SpecificGasConstantJoulesPerKilogramKelvin: 1),
            AmbientPressurePascals: 0);

        var outcome = _calculator.Calculate(inputs);

        Assert.False(outcome.IsSuccess);
        Assert.Null(outcome.Performance);
        Assert.Equal(
            CalculationDiagnosticCodes.NumericFailure,
            Assert.Single(outcome.Issues).Code);
    }

    [Fact]
    public void Calculate_WithNullInputs_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => _calculator.Calculate(null!));
    }

    private static EngineInputs CreateVc04Inputs()
    {
        return new EngineInputs(
            PropellantLabel: "VC-04 synthetic constant-property gas",
            ChamberPressurePascals: 8_000_000,
            MixtureRatio: 3.5,
            Nozzle: new NozzleGeometry(
                ThroatDiameterMeters: 0.05,
                ExitDiameterMeters: 0.31622776601683794),
            Gas: new GasProperties(
                ChamberTemperatureKelvin: 3500,
                SpecificHeatRatio: 1.22,
                SpecificGasConstantJoulesPerKilogramKelvin: 355),
            AmbientPressurePascals:
                PhysicalConstants.StandardSeaLevelPressurePascals,
            TargetMassFlowRateKilogramsPerSecond: 20,
            BurnDurationSeconds: 10);
    }

    private static EnginePerformanceResult AssertSuccess(
        EngineCalculationResult outcome)
    {
        Assert.True(outcome.IsSuccess);
        Assert.NotNull(outcome.Performance);
        return outcome.Performance;
    }

    private static void AssertAmbientPerformance(
        AmbientPerformanceResult actual,
        double expectedPressureThrust,
        double expectedTotalThrust,
        double expectedSpecificImpulse,
        double expectedThrustCoefficient,
        NozzleExpansionState expectedState)
    {
        AssertRelative(
            expected: 28_060.392324969314,
            actual.MomentumThrustNewtons,
            tolerance: 1e-6);
        AssertRelative(
            expectedPressureThrust,
            actual.PressureThrustNewtons,
            tolerance: 1e-6);
        AssertRelative(
            expectedTotalThrust,
            actual.TotalThrustNewtons,
            tolerance: 1e-6);
        AssertRelative(
            expectedSpecificImpulse,
            actual.SpecificImpulseSeconds,
            tolerance: 1e-6);
        AssertRelative(
            expectedThrustCoefficient,
            actual.ThrustCoefficient,
            tolerance: 1e-6);
        Assert.Equal(expectedState, actual.NozzleExpansionState);
    }

    private static void AssertRelative(
        double expected,
        double actual,
        double tolerance = 1e-8)
    {
        var scale = Math.Abs(expected);
        var error = Math.Abs(actual - expected);
        var comparisonError = scale == 0 ? error : error / scale;

        Assert.True(
            comparisonError <= tolerance,
            $"Expected {expected:R}, actual {actual:R}, "
                + $"comparison error {comparisonError:R}.");
    }
}
