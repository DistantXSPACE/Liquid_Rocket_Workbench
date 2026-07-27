using LiquidRocketWorkbench.Core.Calculations;
using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.Core.Tests.Calculations;

public sealed class AmbientPerformanceCalculatorTests
{
    private const double ChamberPressurePascals = 8_000_000;
    private const double MassFlowRateKilogramsPerSecond =
        9.193408634242926;

    [Fact]
    public void Calculate_WithVc04Vacuum_MatchesFixedPerformance()
    {
        var result = AmbientPerformanceCalculator.Calculate(
            ambientPressurePascals: 0,
            ChamberPressurePascals,
            MassFlowRateKilogramsPerSecond,
            CreateVc04Geometry(),
            CreateVc04Exit());

        Assert.Equal(0, result.AmbientPressurePascals);
        AssertRelative(
            expected: 28_060.392324969314,
            result.MomentumThrustNewtons);
        AssertRelative(
            expected: 1212.412734847917,
            result.PressureThrustNewtons);
        AssertRelative(
            expected: 29_272.80505981723,
            result.TotalThrustNewtons);
        AssertRelative(
            expected: 324.6886449456306,
            result.SpecificImpulseSeconds);
        AssertRelative(
            expected: 1.8635646493741427,
            result.ThrustCoefficient);
        Assert.Equal(
            NozzleExpansionState.Underexpanded,
            result.NozzleExpansionState);
    }

    [Fact]
    public void Calculate_WithVc04SeaLevel_MatchesFixedPerformance()
    {
        var result = AmbientPerformanceCalculator.Calculate(
            PhysicalConstants.StandardSeaLevelPressurePascals,
            ChamberPressurePascals,
            MassFlowRateKilogramsPerSecond,
            CreateVc04Geometry(),
            CreateVc04Exit());

        Assert.Equal(
            PhysicalConstants.StandardSeaLevelPressurePascals,
            result.AmbientPressurePascals);
        AssertRelative(
            expected: 28_060.392324969314,
            result.MomentumThrustNewtons);
        AssertRelative(
            expected: -6745.634155776728,
            result.PressureThrustNewtons);
        AssertRelative(
            expected: 21_314.758169192588,
            result.TotalThrustNewtons);
        AssertRelative(
            expected: 236.41943206867248,
            result.SpecificImpulseSeconds);
        AssertRelative(
            expected: 1.3569396493741428,
            result.ThrustCoefficient);
        Assert.Equal(
            NozzleExpansionState.Overexpanded,
            result.NozzleExpansionState);
    }

    [Fact]
    public void Calculate_WhenPressuresMatch_ClassifiesIdeallyExpanded()
    {
        var result = AmbientPerformanceCalculator.Calculate(
            ambientPressurePascals: 15_436.918385488754,
            ChamberPressurePascals,
            MassFlowRateKilogramsPerSecond,
            CreateVc04Geometry(),
            CreateVc04Exit());

        Assert.Equal(0, result.PressureThrustNewtons);
        Assert.Equal(
            NozzleExpansionState.IdeallyExpanded,
            result.NozzleExpansionState);
    }

    [Fact]
    public void Calculate_AllowsFiniteNegativeIdealPerformance()
    {
        var result = AmbientPerformanceCalculator.Calculate(
            ambientPressurePascals: 100,
            chamberPressurePascals: 10,
            massFlowRateKilogramsPerSecond: 1,
            geometry: new NozzleGeometryResult(1, 1, 1),
            nozzleExit: new NozzleExitResult(1, 0, 1, 1));

        Assert.Equal(1, result.MomentumThrustNewtons);
        Assert.Equal(-100, result.PressureThrustNewtons);
        Assert.Equal(-99, result.TotalThrustNewtons);
        Assert.True(result.SpecificImpulseSeconds < 0);
        Assert.True(result.ThrustCoefficient < 0);
    }

    [Theory]
    [InlineData(-1, 1, 1)]
    [InlineData(double.NaN, 1, 1)]
    [InlineData(0, 0, 1)]
    [InlineData(0, double.PositiveInfinity, 1)]
    [InlineData(0, 1, 0)]
    [InlineData(0, 1, double.NaN)]
    public void Calculate_WithInvalidScalarInput_Throws(
        double ambientPressure,
        double chamberPressure,
        double massFlowRate)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => AmbientPerformanceCalculator.Calculate(
                ambientPressure,
                chamberPressure,
                massFlowRate,
                CreateVc04Geometry(),
                CreateVc04Exit()));
    }

    [Fact]
    public void Calculate_WithNullGeometry_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => AmbientPerformanceCalculator.Calculate(
                ambientPressurePascals: 0,
                ChamberPressurePascals,
                MassFlowRateKilogramsPerSecond,
                geometry: null!,
                CreateVc04Exit()));
    }

    [Fact]
    public void Calculate_WithNullNozzleExit_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => AmbientPerformanceCalculator.Calculate(
                ambientPressurePascals: 0,
                ChamberPressurePascals,
                MassFlowRateKilogramsPerSecond,
                CreateVc04Geometry(),
                nozzleExit: null!));
    }

    [Theory]
    [InlineData(0, 1, 1)]
    [InlineData(1, 0, 1)]
    [InlineData(1, 0.5, 0.5)]
    [InlineData(1, 2, 1.5)]
    [InlineData(1, double.NaN, 1)]
    public void Calculate_WithInvalidGeometry_Throws(
        double throatArea,
        double exitArea,
        double expansionRatio)
    {
        var geometry = new NozzleGeometryResult(
            throatArea,
            exitArea,
            expansionRatio);

        Assert.ThrowsAny<ArgumentException>(
            () => AmbientPerformanceCalculator.Calculate(
                ambientPressurePascals: 0,
                ChamberPressurePascals,
                MassFlowRateKilogramsPerSecond,
                geometry,
                CreateVc04Exit()));
    }

    [Theory]
    [InlineData(0, 1, 1, 1)]
    [InlineData(1, -1, 1, 1)]
    [InlineData(1, double.NaN, 1, 1)]
    [InlineData(1, 1, 0, 1)]
    [InlineData(1, 1, 1, double.PositiveInfinity)]
    public void Calculate_WithInvalidNozzleExit_Throws(
        double machNumber,
        double pressure,
        double temperature,
        double velocity)
    {
        var nozzleExit = new NozzleExitResult(
            machNumber,
            pressure,
            temperature,
            velocity);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => AmbientPerformanceCalculator.Calculate(
                ambientPressurePascals: 0,
                ChamberPressurePascals,
                MassFlowRateKilogramsPerSecond,
                CreateVc04Geometry(),
                nozzleExit));
    }

    [Fact]
    public void Calculate_WhenMomentumThrustOverflows_Throws()
    {
        var nozzleExit = new NozzleExitResult(
            MachNumber: 1,
            PressurePascals: 1,
            TemperatureKelvin: 1,
            VelocityMetersPerSecond: double.MaxValue);

        Assert.Throws<OverflowException>(
            () => AmbientPerformanceCalculator.Calculate(
                ambientPressurePascals: 0,
                chamberPressurePascals: 1,
                massFlowRateKilogramsPerSecond: 2,
                geometry: new NozzleGeometryResult(1, 1, 1),
                nozzleExit));
    }

    private static NozzleGeometryResult CreateVc04Geometry()
    {
        return new NozzleGeometryResult(
            ThroatAreaSquareMeters: 0.001963495408493621,
            ExitAreaSquareMeters: 0.07853981633974483,
            ExpansionRatio: 40);
    }

    private static NozzleExitResult CreateVc04Exit()
    {
        return new NozzleExitResult(
            MachNumber: 4.355537019605814,
            PressurePascals: 15_436.918385488754,
            TemperatureKelvin: 1133.8686466837182,
            VelocityMetersPerSecond: 3052.229422333306);
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
