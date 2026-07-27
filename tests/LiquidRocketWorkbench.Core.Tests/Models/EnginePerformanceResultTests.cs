using LiquidRocketWorkbench.Core.Diagnostics;
using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.Core.Tests.Models;

public sealed class EnginePerformanceResultTests
{
    [Fact]
    public void Constructor_CopiesDiagnosticCollection()
    {
        var sourceDiagnostics = new List<ValidationIssue>
        {
            new(
                "MODEL.IDEAL_FLOW_LIMIT",
                ValidationSeverity.Warning,
                "Ideal-flow assumptions apply."),
        };

        var result = CreateResult(sourceDiagnostics);
        sourceDiagnostics.Add(
            new ValidationIssue(
                "LATE.CHANGE",
                ValidationSeverity.Error,
                "This must not appear in the result."));

        var issue = Assert.Single(result.Diagnostics);
        Assert.Equal("MODEL.IDEAL_FLOW_LIMIT", issue.Code);
    }

    [Fact]
    public void Constructor_UsesEmptyDiagnosticsWhenNoneAreProvided()
    {
        var result = CreateResult();

        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void Constructor_RejectsMissingRequiredResultModel()
    {
        Assert.Throws<ArgumentNullException>(
            () => new EnginePerformanceResult(
                geometry: null!,
                massFlow: CreateMassFlow(),
                nozzleExit: CreateNozzleExit(),
                nozzleFlowProfile: CreateNozzleFlowProfile(),
                thrustAltitudeProfile: CreateThrustAltitudeProfile(),
                selectedAmbientPerformance: CreateAmbientPerformance(),
                vacuumPerformance: CreateAmbientPerformance(),
                seaLevelPerformance: CreateAmbientPerformance(),
                characteristicVelocityMetersPerSecond: 1700));
    }

    [Fact]
    public void Constructor_RejectsMissingNozzleFlowProfile()
    {
        Assert.Throws<ArgumentNullException>(
            () => new EnginePerformanceResult(
                geometry: new NozzleGeometryResult(
                    ThroatAreaSquareMeters: 0.001,
                    ExitAreaSquareMeters: 0.04,
                    ExpansionRatio: 40),
                massFlow: CreateMassFlow(),
                nozzleExit: CreateNozzleExit(),
                nozzleFlowProfile: null!,
                thrustAltitudeProfile: CreateThrustAltitudeProfile(),
                selectedAmbientPerformance: CreateAmbientPerformance(),
                vacuumPerformance: CreateAmbientPerformance(),
                seaLevelPerformance: CreateAmbientPerformance(),
                characteristicVelocityMetersPerSecond: 1700));
    }

    [Fact]
    public void Constructor_RejectsMissingThrustAltitudeProfile()
    {
        Assert.Throws<ArgumentNullException>(
            () => new EnginePerformanceResult(
                geometry: new NozzleGeometryResult(
                    ThroatAreaSquareMeters: 0.001,
                    ExitAreaSquareMeters: 0.04,
                    ExpansionRatio: 40),
                massFlow: CreateMassFlow(),
                nozzleExit: CreateNozzleExit(),
                nozzleFlowProfile: CreateNozzleFlowProfile(),
                thrustAltitudeProfile: null!,
                selectedAmbientPerformance: CreateAmbientPerformance(),
                vacuumPerformance: CreateAmbientPerformance(),
                seaLevelPerformance: CreateAmbientPerformance(),
                characteristicVelocityMetersPerSecond: 1700));
    }

    private static EnginePerformanceResult CreateResult(
        IEnumerable<ValidationIssue>? diagnostics = null)
    {
        return new EnginePerformanceResult(
            geometry: new NozzleGeometryResult(
                ThroatAreaSquareMeters: 0.001,
                ExitAreaSquareMeters: 0.04,
                ExpansionRatio: 40),
            massFlow: CreateMassFlow(),
            nozzleExit: CreateNozzleExit(),
            nozzleFlowProfile: CreateNozzleFlowProfile(),
            thrustAltitudeProfile: CreateThrustAltitudeProfile(),
            selectedAmbientPerformance: CreateAmbientPerformance(),
            vacuumPerformance: CreateAmbientPerformance(),
            seaLevelPerformance: CreateAmbientPerformance(),
            characteristicVelocityMetersPerSecond: 1700,
            diagnostics: diagnostics);
    }

    private static MassFlowResult CreateMassFlow()
    {
        return new MassFlowResult(
            CalculatedMassFlowRateKilogramsPerSecond: 10,
            OxidizerMassFlowRateKilogramsPerSecond: 7.5,
            FuelMassFlowRateKilogramsPerSecond: 2.5,
            TargetMassFlowRateKilogramsPerSecond: null,
            AbsoluteTargetDifferenceKilogramsPerSecond: null,
            RelativeTargetDifference: null,
            PropellantMassConsumedKilograms: null);
    }

    private static NozzleExitResult CreateNozzleExit()
    {
        return new NozzleExitResult(
            MachNumber: 3,
            PressurePascals: 50_000,
            TemperatureKelvin: 1500,
            VelocityMetersPerSecond: 2500);
    }

    private static NozzleFlowProfileResult CreateNozzleFlowProfile()
    {
        return new NozzleFlowProfileResult(
            [
                new NozzleProfilePoint(
                    NormalizedAxialPosition: 0,
                    MachNumber: 0,
                    PressurePascals: 1_000_000,
                    TemperatureKelvin: 300),
                new NozzleProfilePoint(
                    NormalizedAxialPosition: 0.35,
                    MachNumber: 1,
                    PressurePascals: 528_282,
                    TemperatureKelvin: 250),
                new NozzleProfilePoint(
                    NormalizedAxialPosition: 1,
                    MachNumber: 3,
                    PressurePascals: 27_223,
                    TemperatureKelvin: 107),
            ],
            throatIndex: 1);
    }

    private static StandardAtmosphereThrustProfileResult
        CreateThrustAltitudeProfile()
    {
        return new StandardAtmosphereThrustProfileResult(
            [
                new StandardAtmosphereThrustPoint(
                    GeopotentialAltitudeMeters: 0,
                    AmbientPressurePascals: 101_325,
                    TotalThrustNewtons: 23_000,
                    NozzleExpansionState.Overexpanded),
                new StandardAtmosphereThrustPoint(
                    GeopotentialAltitudeMeters: 50_000,
                    AmbientPressurePascals: 75.9448,
                    TotalThrustNewtons: 29_000,
                    NozzleExpansionState.Underexpanded),
            ],
            selectedAmbientEquivalentGeopotentialAltitudeMeters: 0);
    }

    private static AmbientPerformanceResult CreateAmbientPerformance()
    {
        return new AmbientPerformanceResult(
            AmbientPressurePascals: 101_325,
            MomentumThrustNewtons: 25_000,
            PressureThrustNewtons: -2_000,
            TotalThrustNewtons: 23_000,
            SpecificImpulseSeconds: 300,
            ThrustCoefficient: 1.5,
            NozzleExpansionState: NozzleExpansionState.Overexpanded);
    }
}
